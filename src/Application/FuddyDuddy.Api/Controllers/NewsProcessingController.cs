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
    private readonly ILogger<NewsProcessingController> _logger;
    private readonly SummaryValidationService _summaryValidationService;
    private readonly ICacheService _cacheService;
    private readonly INewsSummaryRepository _summaryRepository;

    public NewsProcessingController(
        NewsProcessingService newsProcessingService,
        ILogger<NewsProcessingController> logger,
        SummaryValidationService summaryValidationService,
        ICacheService cacheService,
        INewsSummaryRepository summaryRepository)
    {
        _newsProcessingService = newsProcessingService;
        _logger = logger;
        _summaryValidationService = summaryValidationService;
        _cacheService = cacheService;
        _summaryRepository = summaryRepository;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessNews(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting news processing flow");
        
        try
        {
            await _newsProcessingService.ProcessNewsSourcesAsync(cancellationToken);
            return Ok(new { message = "News processing completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during news processing");
            return StatusCode(500, new { error = "Internal server error during news processing" });
        }
    }

    [HttpPost("validate-summaries")]
    public async Task<IActionResult> ValidateSummaries(CancellationToken cancellationToken)
    {
        await _summaryValidationService.ValidateNewSummariesAsync(cancellationToken);
        return Ok();
    }

    [HttpPost("rebuild-cache")]
    public async Task<IActionResult> RebuildCache(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting cache rebuild");

            // Get all validated summaries from the database
            var summaries = await _summaryRepository.GetByStateAsync(NewsSummaryState.Validated, cancellationToken);
            
            // Rebuild the cache with these summaries
            await _cacheService.RebuildCacheAsync(summaries, cancellationToken);

            _logger.LogInformation("Cache rebuild completed successfully");
            return Ok(new { message = "Cache rebuilt successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding cache");
            return StatusCode(500, new { error = "Internal server error during cache rebuild" });
        }
    }
} 