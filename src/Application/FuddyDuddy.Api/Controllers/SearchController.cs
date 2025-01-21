using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ICacheService _cacheService;

    public SearchController(IVectorSearchService vectorSearchService, ICacheService cacheService)
    {
        _vectorSearchService = vectorSearchService;
        _cacheService = cacheService;
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
        var results = await _vectorSearchService.SearchAsync(request.Query, request.Language, request.Limit, cancellationToken);
        var summaries = await _cacheService.GetSummariesByIdsAsync<CachedSummaryDto>(results.Select(r => r.SummaryId), cancellationToken);
        return Ok(results.Select(r => new SearchResult { Summary = summaries.First(s => s.Id == r.SummaryId), Score = r.Score }).OrderByDescending(r => r.Score));
    }

    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int Limit { get; set; } = 10;
        public Language Language { get; set; } = Language.RU;
    }

    public class SearchResult
    {
        public CachedSummaryDto Summary { get; set; }
        public float Score { get; set; }
    }
}