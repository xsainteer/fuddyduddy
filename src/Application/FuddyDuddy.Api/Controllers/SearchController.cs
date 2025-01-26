using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Configuration;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using Microsoft.Extensions.Options;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ICacheService _cacheService;
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly IOptions<SearchSettings> _searchSettings;

    public SearchController(
        IVectorSearchService vectorSearchService,
        ICacheService cacheService,
        INewsSummaryRepository summaryRepository,
        IOptions<SearchSettings> searchSettings)
    {
        _vectorSearchService = vectorSearchService;
        _cacheService = cacheService;
        _summaryRepository = summaryRepository;
        _searchSettings = searchSettings;
    }

    [HttpPost("summaries")]
    public async Task<IActionResult> SearchSummariesAsync(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Query == null)
        {
            return BadRequest(new { message = "Query is required" });
        }
        if (request.Query.Length < _searchSettings.Value.MinQueryLength || request.Query.Length > _searchSettings.Value.MaxQueryLength)
        {
            return BadRequest(new { message = $"Query must be at least {_searchSettings.Value.MinQueryLength} characters long and less than {_searchSettings.Value.MaxQueryLength} characters" });
        }
        if (request.Limit <= 0 || request.Limit > 100)
        {
            return BadRequest(new { message = "Limit must be greater than 0 and less than 100" });
        }
        if (!_searchSettings.Value.Enabled)
        {
            return BadRequest(new { message = "Search service is disabled" });
        }

        if (request.FromDate > request.ToDate)
        {
            return BadRequest(new { message = "From date must be less than to date" });
        }

        // Get search results from vector search
        var searchResults = await _vectorSearchService.SearchAsync(
            request.Query,
            request.LanguageEnum,
            request.FromDate,
            request.ToDate,
            request.CategoryIds,
            request.SourceIds,
            request.Limit ?? _searchSettings.Value.MaxSearchResults, 
            cancellationToken
        );

        // Try to get summaries from cache first
        var summaryIds = searchResults.Select(r => r.SummaryId).ToList();
        var cachedSummaries = await _cacheService.GetSummariesByIdsAsync<CachedSummaryDto>(summaryIds, cancellationToken);
        var cachedIds = cachedSummaries.Select(s => s.Id).ToHashSet();

        // Get missing summaries from repository
        var missingIds = summaryIds.Where(id => !cachedIds.Contains(id)).ToList();
        var results = new List<SearchResult>();

        // Add cached summaries
        results.AddRange(searchResults
            .Where(r => cachedIds.Contains(r.SummaryId))
            .Select(r => new SearchResult 
            { 
                Summary = cachedSummaries.First(s => s.Id == r.SummaryId), 
                Score = r.Score 
            }));

        // Add missing summaries from repository if any
        if (missingIds.Any())
        {
            foreach (var id in missingIds)
            {
                var summary = await _summaryRepository.GetByIdAsync(id, cancellationToken);
                if (summary != null)
                {
                    // Include all references to get the full summary data
                    // summary = await _summaryRepository.IncludeAllReferencesAsync(summary, cancellationToken);
                    
                    results.Add(new SearchResult
                    {
                        Summary = CachedSummaryDto.FromNewsSummary(summary),
                        Score = searchResults.First(r => r.SummaryId == id).Score
                    });
                }
            }
        }

        return Ok(results);
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int? Limit { get; set; }
        public string Language { get; set; } = "RU";
        public IEnumerable<int>? CategoryIds { get; set; }
        public IEnumerable<Guid>? SourceIds { get; set; }
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.UtcNow;
        public Language LanguageEnum => Enum.Parse<Language>(Language);
    }

    public class SearchResult
    {
        public CachedSummaryDto Summary { get; set; }
        public float Score { get; set; }
    }
}