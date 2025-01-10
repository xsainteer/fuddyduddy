using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Services;

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
        SummaryTranslationService translationService,
        ILogger<SummariesController> logger)
    {
        _cacheService = cacheService;
        _summaryRepository = summaryRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestSummaries(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 20,
        [FromQuery] Language? language = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] Guid? sourceId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summaries = await _cacheService.GetLatestSummariesAsync<CachedSummaryDto>(
                page * pageSize, 
                pageSize,
                language ?? Language.RU,
                categoryId,
                sourceId,
                cancellationToken);

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summaries. Page: {Page}", page);
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
                        summary = CachedSummaryDto.FromNewsSummary(dbSummary);
                        
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