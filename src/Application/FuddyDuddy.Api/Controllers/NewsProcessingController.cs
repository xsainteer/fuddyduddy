using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsProcessingController : ControllerBase
{
    private readonly NewsProcessingService _newsProcessingService;
    private readonly SummaryValidationService _validationService;
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<NewsProcessingController> _logger;

    public NewsProcessingController(
        NewsProcessingService newsProcessingService,
        SummaryValidationService validationService,
        INewsSummaryRepository summaryRepository,
        ICacheService cacheService,
        ILogger<NewsProcessingController> logger)
    {
        _newsProcessingService = newsProcessingService;
        _validationService = validationService;
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessNews(CancellationToken cancellationToken = default)
    {
        try
        {
            await _newsProcessingService.ProcessNewsSourcesAsync(cancellationToken);
            return Ok(new { message = "News processing started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing news");
            return StatusCode(500, new { message = "An error occurred while processing news" });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateSummaries(CancellationToken cancellationToken = default)
    {
        try
        {
            await _validationService.ValidateNewSummariesAsync(cancellationToken);
            return Ok(new { message = "Summaries validation started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating summaries");
            return StatusCode(500, new { message = "An error occurred while validating summaries" });
        }
    }

    [HttpPost("rebuild-cache")]
    public async Task<IActionResult> RebuildCache(CancellationToken cancellationToken = default)
    {
        try
        {
            // Clear existing cache
            await _cacheService.ClearCacheAsync(cancellationToken);

            // Get all validated summaries
            var summaries = await _summaryRepository.GetByStateAsync(NewsSummaryState.Validated, cancellationToken);

            // Add each summary back to cache
            foreach (var summary in summaries)
            {
                await _cacheService.AddSummaryAsync(summary, cancellationToken);
            }

            return Ok(new { message = $"Cache rebuilt with {summaries.Count()} summaries" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding cache");
            return StatusCode(500, new { message = "An error occurred while rebuilding cache" });
        }
    }
} 