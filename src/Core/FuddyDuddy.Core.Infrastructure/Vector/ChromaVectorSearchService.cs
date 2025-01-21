using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChromaDB.Client;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Infrastructure.AI;
using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Infrastructure.Vector;

internal sealed class ChromaVectorSearchService : IVectorSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<ChromaVectorSearchService> _logger;
    private readonly IOptions<ChromaDbOptions> _chromaOptions;
    private readonly INewsSummaryRepository _newsSummaryRepository;

    public ChromaVectorSearchService(
        IEmbeddingService embeddingService,
        IOptions<ChromaDbOptions> chromaOptions,
        ILogger<ChromaVectorSearchService> logger,
        IHttpClientFactory httpClientFactory,
        INewsSummaryRepository newsSummaryRepository)
    {
        _httpClientFactory = httpClientFactory;
        _embeddingService = embeddingService;
        _logger = logger;
        _chromaOptions = chromaOptions;
        _newsSummaryRepository = newsSummaryRepository;
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

            // Generate embedding for the summary
            var embedding = await _embeddingService.GenerateEmbeddingAsync(
                $"{summary.NewsArticle.NewsSource.Name}, {summary.Category.Name}\n{summary.GeneratedAt:f}\n{summary.Title}\n\n{summary.Article}", 
                cancellationToken);

            if (embedding == null)
            {
                _logger.LogError("Embedding for summary {SummaryId} is null", summary.Id);
                throw new Exception("Embedding is null");
            }

            // Prepare metadata
            var metadata = new Dictionary<string, object>
            {
                { "id", summary.Id.ToString() }
            };

            // Add to ChromaDB
            await collectionClient.Add(
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

            // Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            // Search in ChromaDB with filtering by metadata
            var queryResults = await collectionClient.Query(
                queryEmbeddings: new List<ReadOnlyMemory<float>> { queryEmbedding.AsMemory() },
                nResults: limit,
                include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Documents | ChromaQueryInclude.Distances);

            // Filter results in memory since ChromaDB's where clause is not working as expected
            var summaries = new List<(Guid SummaryId, float Score)>();
            
            // ChromaDB returns a list of query results, each containing a list of entries
            // Since we only have one query embedding, we only need to process the first result
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

                // Use the distance as score (cosine similarity)
                var score = entry.Distance;
                summaries.Add((summaryId, score));
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