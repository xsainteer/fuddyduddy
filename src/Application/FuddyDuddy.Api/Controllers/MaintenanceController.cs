using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Runtime.CompilerServices;
using FuddyDuddy.Core.Application.Models;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly INewsProcessingService _newsProcessingService;
    private readonly ISummaryValidationService _validationService;
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<MaintenanceController> _logger;
    private readonly ISummaryTranslationService _translationService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly IDigestRepository _digestRepository;
    public MaintenanceController(
        INewsProcessingService newsProcessingService,
        ISummaryValidationService validationService,
        INewsSummaryRepository summaryRepository,
        ICacheService cacheService,
        ILogger<MaintenanceController> logger,
        ISummaryTranslationService translationService,
        IMaintenanceService maintenanceService,
        IDigestRepository digestRepository)
    {
        _newsProcessingService = newsProcessingService;
        _validationService = validationService;
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _translationService = translationService;
        _maintenanceService = maintenanceService;
        _digestRepository = digestRepository;
    }

    [HttpPost("process-news")]
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

    [HttpPost("validate-summaries")]
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

    [HttpGet("revisit-categories")]
    public async Task RevisitCategories(string since, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        // Response.Headers.Add("Cache-Control", "no-cache");
        // Response.Headers.Add("Connection", "keep-alive");
        
        var sinceDate = DateTimeOffset.ParseExact(since, "yyyyMMddTHHmm", null);
        
        await foreach (var result in _maintenanceService.RevisitCategoriesAsync(sinceDate, cancellationToken))
        {
            await Response.WriteAsync($"data: {result}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
        
        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
    }

    [HttpPost("rebuild-cache")]
    public async Task<IActionResult> RebuildCache(CancellationToken cancellationToken = default)
    {
        try
        {
            // Clear existing cache
            await _cacheService.ClearCacheAsync(cancellationToken);

            // Get all validated summaries
            var summaries = await _summaryRepository.GetValidatedOrDigestedAsync(cancellationToken);

            // Add each summary back to cache
            foreach (var summary in summaries)
            {
                await _cacheService.AddSummaryAsync(summary, cancellationToken);
            }

            // Add digests back to cache
            var digests = await _digestRepository.GetLatestAsync(100, cancellationToken);
            foreach (var digest in digests)
            {
                await _cacheService.AddDigestAsync(CachedDigestDto.FromDigest(digest), cancellationToken);
            }

            return Ok(new { message = $"Cache rebuilt with {summaries.Count()} summaries" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding cache");
            return StatusCode(500, new { message = "An error occurred while rebuilding cache" });
        }
    }

    [HttpPost("translate-summary/{id}")]
    public async Task<IActionResult> TranslateSummary(
        string id, 
        [FromQuery] Language targetLanguage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(id, out var summaryId))
            {
                return BadRequest(new { message = "Invalid summary ID format" });
            }

            var translatedSummary = await _translationService.TranslateSummaryAsync(summaryId, targetLanguage, cancellationToken);
            if (translatedSummary == null)
            {
                return NotFound(new { message = "Summary not found or translation failed" });
            }

            return Ok(translatedSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating summary. Id: {Id}, TargetLanguage: {Language}", id, targetLanguage);
            return StatusCode(500, new { message = "An error occurred while translating the summary" });
        }
    }
} 