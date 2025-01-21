using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Models.Broker;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application;

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
    private readonly ISimilarRepository _similarRepository;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IBroker _broker;
    public MaintenanceController(
        INewsProcessingService newsProcessingService,
        ISummaryValidationService validationService,
        INewsSummaryRepository summaryRepository,
        ICacheService cacheService,
        ILogger<MaintenanceController> logger,
        ISummaryTranslationService translationService,
        IMaintenanceService maintenanceService,
        IDigestRepository digestRepository,
        ISimilarRepository similarRepository,
        IVectorSearchService vectorSearchService,
        IBroker broker)
    {
        _newsProcessingService = newsProcessingService;
        _validationService = validationService;
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _translationService = translationService;
        _maintenanceService = maintenanceService;
        _digestRepository = digestRepository;
        _similarRepository = similarRepository;
        _vectorSearchService = vectorSearchService;
        _broker = broker;
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
    public async Task RebuildCache(CancellationToken cancellationToken = default)
    {
        Response.ContentType = "text/event-stream";

        // Clear existing cache
        await _cacheService.ClearCacheAsync(cancellationToken);

        await foreach (var progress in RebuildingSummariesCache(cancellationToken))
        {
            await Response.WriteAsync($"data: {progress}\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        // Add digests back to cache
        await foreach (var progress in RebuildingDigestsCache(cancellationToken))
        {
            await Response.WriteAsync($"data: {progress}\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
    }


    [HttpPost("rebuild-vector-index")]
    public async Task<IActionResult> RebuildVectorIndex(CancellationToken cancellationToken = default)
    {
        var summaries = await _summaryRepository.GetValidatedOrDigestedAsync(cancellationToken: cancellationToken);
        // Delete summaries from vector index
        foreach (var summary in summaries)
        {
            await _broker.PushAsync(QueueNameConstants.Index, new IndexRequest(summary.Id, IndexRequestType.Delete), cancellationToken);
        }
        // Add summaries to vector index
        foreach (var summary in summaries)
        {
            await _broker.PushAsync(QueueNameConstants.Index, new IndexRequest(summary.Id, IndexRequestType.Add), cancellationToken);
        }

        return Ok(new { message = "Vector index rebuilt" });
    }

    [HttpPost("delete-similar/{id}")]
    public async Task<IActionResult> DeleteSimilar(string id, CancellationToken cancellationToken = default)
    {
        var similarId = Guid.Parse(id);
        var newsSummaryIds = await _similarRepository.DeleteSimilarAsync(similarId, cancellationToken);
        foreach (var newsSummaryId in newsSummaryIds)
        {
            await _cacheService.AddSummaryAsync(newsSummaryId, cancellationToken);
        }
        return Ok(new { message = $"Similar {similarId} deleted. News summaries ({string.Join(", ", newsSummaryIds)}) updated in cache." });
    }

    [HttpPost("delete-similar-reference/{id}")]
    public async Task<IActionResult> DeleteSimilarReference(string id, CancellationToken cancellationToken = default)
    {
        var summaryId = Guid.Parse(id);
        var newsSummaryIds = await _similarRepository.DeleteSimilarReferenceAsync(summaryId, cancellationToken);
        foreach (var newsSummaryId in newsSummaryIds)
        {
            await _cacheService.AddSummaryAsync(newsSummaryId, cancellationToken);
        }
        return Ok(new { message = $"Similar reference for newssummary {summaryId} deleted. News summaries ({string.Join(", ", newsSummaryIds)}) updated in cache." });
    }

    [HttpPost("update-cache/summaries/{id}")]
    public async Task<IActionResult> UpdateCache(string id, CancellationToken cancellationToken = default)
    {
        var summaryId = Guid.Parse(id);
        await _cacheService.AddSummaryAsync(summaryId, cancellationToken);
        return Ok(new { message = $"Summary {summaryId} updated in cache." });
    }

    private async IAsyncEnumerable<string> RebuildingDigestsCache([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var digests = (await _digestRepository.GetLatestAsync(100, cancellationToken)).ToArray();
        var k = 0;
        foreach (var digest in digests)
        {
            await _cacheService.AddDigestAsync(CachedDigestDto.FromDigest(digest), cancellationToken);
            yield return $"{k++ * 100 / digests.Length}% digests completed";
        }
        yield return "DONE. 100% of digests added to cache.";
    }

    private async IAsyncEnumerable<string> RebuildingSummariesCache([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var summaries = (await _summaryRepository.GetValidatedOrDigestedAsync(2000, cancellationToken)).ToArray();
        var k = 0;
        foreach (var summary in summaries)
        {
            await _cacheService.AddSummaryAsync(summary.Id, cancellationToken);
            yield return $"{k++ * 100 / summaries.Length}% summaries completed";
        }
        yield return "DONE. 100% of summaries added to cache.";
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