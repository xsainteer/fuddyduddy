using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Configuration;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ICacheService _cacheService;
    private readonly IOptions<SearchSettings> _searchSettings;

    public SearchController(
        IVectorSearchService vectorSearchService,
        ICacheService cacheService,
        IOptions<SearchSettings> searchSettings)
    {
        _vectorSearchService = vectorSearchService;
        _cacheService = cacheService;
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
        if (request.Limit <= 0 || request.Limit > 100)
        {
            return BadRequest(new { message = "Limit must be greater than 0 and less than 100" });
        }
        if (!_searchSettings.Value.Enabled)
        {
            return BadRequest(new { message = "Search service is disabled" });
        }
        var results = await _vectorSearchService.SearchAsync(request.Query, request.LanguageEnum, request.Limit ?? _searchSettings.Value.MaxSearchResults, cancellationToken);
        var summaries = await _cacheService.GetSummariesByIdsAsync<CachedSummaryDto>(results.Select(r => r.SummaryId), cancellationToken);
        return Ok(results.Select(r => new SearchResult { Summary = summaries.First(s => s.Id == r.SummaryId), Score = r.Score }).OrderBy(r => r.Score));
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int? Limit { get; set; }
        public string Language { get; set; } = "RU";
        public Language LanguageEnum => Enum.Parse<Language>(Language);
    }

    public class SearchResult
    {
        public CachedSummaryDto Summary { get; set; }
        public float Score { get; set; }
    }
}