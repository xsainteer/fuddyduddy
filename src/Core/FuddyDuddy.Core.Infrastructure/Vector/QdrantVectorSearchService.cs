using System.Text.RegularExpressions;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Infrastructure.AI;
using FuddyDuddy.Core.Infrastructure.Configuration;
using FuddyDuddy.Core.Infrastructure.RateLimit;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Range = Qdrant.Client.Grpc.Range;
namespace FuddyDuddy.Core.Infrastructure.Vector;

internal sealed class QdrantVectorSearchService : IVectorSearchService
{
    private const string LEAKY_BUCKET_KEY = "qdrant_rate_limit:{0}";
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<QdrantVectorSearchService> _logger;
    private readonly IOptions<QdrantOptions> _qdrantOptions;
    private readonly INewsSummaryRepository _newsSummaryRepository;
    private readonly IRateLimiter _rateLimiter;
    private readonly QdrantClient _qdrantClient;

    public QdrantVectorSearchService(
        IEmbeddingService embeddingService,
        IOptions<QdrantOptions> qdrantOptions,
        ILogger<QdrantVectorSearchService> logger,
        INewsSummaryRepository newsSummaryRepository,
        IRateLimiter rateLimiter,
        ILoggerFactory loggerFactory)
    {
        _embeddingService = embeddingService;
        _logger = logger;
        _qdrantOptions = qdrantOptions;
        _newsSummaryRepository = newsSummaryRepository;
        _rateLimiter = rateLimiter;
        
        _qdrantClient = new QdrantClient(_qdrantOptions.Value.Host, _qdrantOptions.Value.Port, loggerFactory: loggerFactory);
    }

    private async Task EnsureCollectionExists(Language language, CancellationToken cancellationToken)
    {
        try
        {
            var collectionName = $"{_qdrantOptions.Value.CollectionName}_{language.GetDescription()}";
            var collectionExists = await _qdrantClient.CollectionExistsAsync(collectionName, cancellationToken);
            if (!collectionExists)
            {
                var vectorParams = new VectorParams 
                { 
                    Size = (ulong)_qdrantOptions.Value.VectorSize,
                    Distance = _qdrantOptions.Value.Distance,
                    OnDisk = false
                };
                
                await _qdrantClient.CreateCollectionAsync(
                    collectionName,
                    vectorParams);

                _logger.LogInformation("Created Qdrant collection {CollectionName}", collectionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring collection exists in Qdrant");
            throw;
        }
    }

    private async Task CheckRateLimit(CancellationToken cancellationToken)
    {
        if (_qdrantOptions.Value.RatePerMinute > 0)
        {
            var waitTime = await _rateLimiter.GetMinuteTokenAsync(
                string.Format(LEAKY_BUCKET_KEY, _qdrantOptions.Value.CollectionName),
                _qdrantOptions.Value.RatePerMinute,
                120);
            if (waitTime == -1)
            {
                _logger.LogError("Rate limit exceeded, it requires to wait for 2 minutes, so rejecting request");
                throw new Exception("Rate limit exceeded");
            }
            if (waitTime > 0)
            {
                _logger.LogWarning("Rate limit exceeded, waiting for {WaitTime} seconds", waitTime);
                await Task.Delay((int)waitTime * 1000, cancellationToken);
            }
        }
    }

    public async Task IndexSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _newsSummaryRepository.GetByIdAsync(summaryId, cancellationToken);
            if (summary == null)
            {
                _logger.LogError("Summary {SummaryId} not found", summaryId);
                throw new Exception("Summary not found");
            }

            await CheckRateLimit(cancellationToken);

            await EnsureCollectionExists(summary.Language, cancellationToken);

            var embedding = await _embeddingService.GenerateEmbeddingAsync(
                $"{summary.Title}\n{summary.Article}",
                cancellationToken);

            if (embedding == null)
            {
                _logger.LogError("Embedding for summary {SummaryId} is null", summary.Id);
                throw new Exception("Embedding is null");
            }

            var collectionName = $"{_qdrantOptions.Value.CollectionName}_{summary.Language.GetDescription()}";
            var point = new PointStruct
            {
                Id = new PointId(summaryId),
                Vectors = embedding.ToArray()
            };
            point.Payload.Add("timestamp", new Value { IntegerValue = ((DateTimeOffset)summary.GeneratedAt.ToUniversalTime()).ToUnixTimeSeconds() });
            point.Payload.Add("source", new Value { StringValue = summary.NewsArticle.NewsSourceId.ToString() });
            point.Payload.Add("category", new Value { StringValue = summary.CategoryId.ToString() });

            await _qdrantClient.UpsertAsync(
                collectionName,
                new[] { point });

            _logger.LogInformation("Indexed summary {SummaryId} in Qdrant", summary.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing summary {SummaryId} in Qdrant", summaryId);
            throw;
        }
    }

    public async Task<IEnumerable<(Guid SummaryId, float Score)>> SearchAsync(
        string query,
        Language language,
        DateTime? fromDate,
        DateTime? toDate,
        IEnumerable<int>? categoryIds,
        IEnumerable<Guid>? sourceIds,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await CheckRateLimit(cancellationToken);

            await EnsureCollectionExists(language, cancellationToken);

            var filter = new Filter();

            if (fromDate != DateTime.MinValue || toDate != DateTime.MinValue)
            {
                // Add date range filter if present

                // Add date range filter if present
                if (fromDate.HasValue)
                {
                    var fromDateOffset = ((DateTimeOffset)fromDate.Value.ToUniversalTime());
                    _logger.LogInformation("From date: {FromDateOffset}", fromDateOffset);
                    filter.Must.Add(new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "timestamp",
                            Range = new Range
                            {
                                Gte = fromDateOffset.ToUnixTimeSeconds()
                            }
                        }
                    });
                }
                
                if (toDate.HasValue)
                {
                    var toDateOffset = ((DateTimeOffset)toDate.Value.ToUniversalTime());
                    _logger.LogInformation("To date: {ToDateOffset}", toDateOffset);
                    filter.Must.Add(new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "timestamp",
                            Range = new Range
                            {
                                Lte = (toDateOffset.AddDays(1)).ToUnixTimeSeconds()
                            }
                        }
                    });
                }

                if (categoryIds != null && categoryIds.Any())
                {
                    var categoryFilter = new Filter();
                    foreach (var categoryId in categoryIds)
                    {
                        categoryFilter.Should.Add(new Condition { Field = new FieldCondition { Key = "category", Match = new() { Keyword = categoryId.ToString()} } });
                    }
                    filter.Must.Add(new Condition { Filter = categoryFilter });
                }

                if (sourceIds != null && sourceIds.Any())
                {
                    var sourceFilter = new Filter();
                    foreach (var sourceId in sourceIds)
                    {
                        sourceFilter.Should.Add(new Condition { Field = new FieldCondition { Key = "source", Match = new() { Keyword = sourceId.ToString()} } });
                    }
                    filter.Must.Add(new Condition { Filter = sourceFilter });
                }
            }

            // Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
            
            var collectionName = $"{_qdrantOptions.Value.CollectionName}_{language.GetDescription()}";

            var searchResult = await _qdrantClient.SearchAsync(
                collectionName,
                queryEmbedding.ToArray(),
                limit: (ulong)limit,
                filter: filter);

            return searchResult.Select(r => (Guid.ParseExact(r.Id.Uuid, "D"), r.Score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching in Qdrant with query: {Query}", query);
            throw;
        }
    }

    public async Task DeleteSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default)
    {
        try
        {
            await CheckRateLimit(cancellationToken);

            var summary = await _newsSummaryRepository.GetByIdAsync(summaryId, cancellationToken);
            if (summary == null)
            {
                _logger.LogError("Summary {SummaryId} not found", summaryId);
                throw new Exception("Summary not found");
            }

            var collectionName = $"{_qdrantOptions.Value.CollectionName}_{summary.Language.GetDescription()}";
            await _qdrantClient.DeleteAsync(collectionName, summaryId);

            _logger.LogInformation("Deleted summary {SummaryId} from Qdrant", summaryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting summary {SummaryId} from Qdrant", summaryId);
            throw;
        }
    }

    public async Task RecreateCollectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var language in Enum.GetValues(typeof(Language)))
            {
                var collectionName = $"{_qdrantOptions.Value.CollectionName}_{((Language)language).GetDescription()}";
                await _qdrantClient.DeleteCollectionAsync(collectionName);
                await EnsureCollectionExists((Language)language, cancellationToken);
                _logger.LogInformation("Recreated collection for language {Language}", ((Language)language).GetDescription());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recreating collection for all languages");
            throw;
        }
    }
} 