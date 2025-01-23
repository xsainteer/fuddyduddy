using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChromaDB.Client;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Infrastructure.AI;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Extensions;

namespace FuddyDuddy.Core.Infrastructure.Vector;

internal sealed class ChromaVectorSearchService : IVectorSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<ChromaVectorSearchService> _logger;
    private readonly IOptions<ChromaDbOptions> _chromaOptions;
    private readonly INewsSummaryRepository _newsSummaryRepository;
    private readonly IDateExtractionService _dateExtractionService;

    public ChromaVectorSearchService(
        IEmbeddingService embeddingService,
        IOptions<ChromaDbOptions> chromaOptions,
        ILogger<ChromaVectorSearchService> logger,
        IHttpClientFactory httpClientFactory,
        INewsSummaryRepository newsSummaryRepository,
        IDateExtractionService dateExtractionService)
    {
        _httpClientFactory = httpClientFactory;
        _embeddingService = embeddingService;
        _logger = logger;
        _chromaOptions = chromaOptions;
        _newsSummaryRepository = newsSummaryRepository;
        _dateExtractionService = dateExtractionService;
    }

    private async Task<ChromaCollectionClient> GetCollectionClient(HttpClient httpClient, Language language)
    {
        var configOptions = new ChromaConfigurationOptions(uri: $"{_chromaOptions.Value.Url}/api/v1/");
        var chromaClient = new ChromaClient(configOptions, httpClient);
        var collection = await chromaClient.GetOrCreateCollection($"{_chromaOptions.Value.CollectionName}_{language.GetDescription()}");
        return new ChromaCollectionClient(collection, configOptions, httpClient);
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

            using var httpClient = _httpClientFactory.CreateClient();
            var collectionClient = await GetCollectionClient(httpClient, summary.Language);

            var categoryName = summary.Language == Language.RU ? summary.Category.Local : summary.Category.Name;
            var embedding = await _embeddingService.GenerateEmbeddingAsync(
                $"Издательство: {summary.NewsArticle.NewsSource.Name}\nКатегория: {categoryName}\nЗаголовок: {summary.Title}\n\nСодержание: {summary.Article}", 
                cancellationToken);

            if (embedding == null)
            {
                _logger.LogError("Embedding for summary {SummaryId} is null", summary.Id);
                throw new Exception("Embedding is null");
            }

            // Add timestamp to metadata
            var metadata = new Dictionary<string, object>
            {
                { "timestamp", ((DateTimeOffset)summary.GeneratedAt.ToUniversalTime()).ToUnixTimeSeconds() }
            };

            await collectionClient.Upsert(
                ids: new List<string> { summary.Id.ToString() },
                embeddings: new List<ReadOnlyMemory<float>> { embedding.AsMemory() },
                metadatas: new List<Dictionary<string, object>> { metadata },
                documents: new List<string> { $"{summary.NewsArticle.NewsSource.Name}, {summary.Category.Name}\n{summary.GeneratedAt:f}\n{summary.Title}\n\n{summary.Article}" });

            _logger.LogInformation("Indexed summary {SummaryId} in ChromaDB", summary.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing summary {SummaryId} in ChromaDB", summaryId);
            throw;
        }
    }

    public async Task<IEnumerable<(Guid SummaryId, float Score)>> SearchAsync(
        string query,
        Language language,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var collectionClient = await GetCollectionClient(httpClient, language);

            // Extract date range from query
            var dateRange = await _dateExtractionService.ExtractDateRangeAsync(query, cancellationToken);
            
            // Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
            
            long? from = DateTimeOffset.TryParse(dateRange.From, out var fromDate) ? fromDate.ToUnixTimeSeconds() : null;
            long? to = DateTimeOffset.TryParse(dateRange.To, out var toDate) ? toDate.AddDays(1).ToUnixTimeSeconds() : null;

            // Build where clause for date filtering
            ChromaWhereOperator? whereOperator = null;
            if (from.HasValue || to.HasValue)
            {
                if (from.HasValue && to.HasValue)
                {
                    whereOperator = ChromaWhereOperator.GreaterThanOrEqual("timestamp", from.Value) &
                                  ChromaWhereOperator.LessThanOrEqual("timestamp", to.Value);
                }
                else if (from.HasValue)
                {
                    whereOperator = ChromaWhereOperator.GreaterThanOrEqual("timestamp", from.Value);
                }
                else if (to.HasValue)
                {
                    whereOperator = ChromaWhereOperator.LessThanOrEqual("timestamp", to.Value);
                }
            }

            // Search in ChromaDB with date filtering
            var queryResults = await collectionClient.Query(
                queryEmbeddings: new List<ReadOnlyMemory<float>> { queryEmbedding.AsMemory() },
                nResults: limit,
                where: whereOperator,
                include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Documents | ChromaQueryInclude.Distances);

            var summaries = new List<(Guid SummaryId, float Score)>();
            var firstQueryResult = queryResults.FirstOrDefault();
            if (firstQueryResult == null)
            {
                return new List<(Guid SummaryId, float Score)>();
            }

            foreach (var entry in firstQueryResult)
            {
                if (!Guid.TryParse(entry.Id, out var summaryId))
                {
                    _logger.LogWarning("Failed to parse summary ID from ChromaDB: {Id}", entry.Id);
                    continue;
                }

                summaries.Add((SummaryId: summaryId, Score: entry.Distance));
            }

            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching in ChromaDB with query: {Query}", query);
            throw;
        }
    }

    public async Task DeleteSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _newsSummaryRepository.GetByIdAsync(summaryId, cancellationToken);
            if (summary == null)
            {
                _logger.LogError("Summary {SummaryId} not found", summaryId);
                throw new Exception("Summary not found");
            }

            using var httpClient = _httpClientFactory.CreateClient();
            var collectionClient = await GetCollectionClient(httpClient, summary.Language);

            await collectionClient.Delete(ids: new List<string> { summaryId.ToString() });
            _logger.LogInformation("Deleted summary {SummaryId} from ChromaDB", summaryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting summary {SummaryId} from ChromaDB", summaryId);
            throw;
        }
    }
} 