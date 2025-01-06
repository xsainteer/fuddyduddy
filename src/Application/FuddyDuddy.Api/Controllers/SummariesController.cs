using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Domain.Repositories;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummariesController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ILogger<SummariesController> _logger;

    public SummariesController(
        ICacheService cacheService,
        INewsSummaryRepository summaryRepository,
        ILogger<SummariesController> logger)
    {
        _cacheService = cacheService;
        _summaryRepository = summaryRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestSummaries(
        [FromQuery] string? summaryId = null,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(summaryId))
            {
                var summariesAroundId = await _cacheService.GetSummariesAroundIdAsync<CachedSummaryDto>(
                    summaryId,
                    pageSize,
                    cancellationToken);

                if (summariesAroundId == null)
                {
                    return NotFound(new { message = "Summary not found" });
                }

                return Ok(summariesAroundId);
            }

            var summaries = await _cacheService.GetLatestSummariesAsync<CachedSummaryDto>(
                page * pageSize, 
                pageSize, 
                cancellationToken);

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summaries. SummaryId: {SummaryId}, Page: {Page}", summaryId, page);
            return StatusCode(500, new { message = "An error occurred while fetching summaries" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSummaryById(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _cacheService.GetSummaryByIdAsync<CachedSummaryDto>(id, cancellationToken);
            
            if (summary == null)
            {
                // Try to get from database if not in cache
                if (Guid.TryParse(id, out var summaryId))
                {
                    var dbSummary = await _summaryRepository.GetByIdAsync(summaryId, cancellationToken);
                    if (dbSummary != null)
                    {
                        // Map to DTO and cache for future requests
                        summary = new CachedSummaryDto
                        {
                            Id = dbSummary.Id,
                            Title = dbSummary.Title,
                            Article = dbSummary.Article,
                            Tags = dbSummary.Tags,
                            GeneratedAt = dbSummary.GeneratedAt
                        };
                        
                        // Cache asynchronously without waiting
                        _ = _cacheService.CacheSummaryDtoAsync(id, summary, cancellationToken);
                        
                        return Ok(summary);
                    }
                }
                
                return NotFound(new { message = "Summary not found" });
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary by ID. Id: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the summary" });
        }
    }
}

public class SummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
} 