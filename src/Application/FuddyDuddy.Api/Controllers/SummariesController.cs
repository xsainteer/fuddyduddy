using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummariesController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<SummariesController> _logger;

    public SummariesController(
        ICacheService cacheService,
        ILogger<SummariesController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestSummaries(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summaries = await _cacheService.GetLatestSummariesAsync<CachedSummaryDto>(
                page * pageSize, 
                pageSize, 
                cancellationToken);

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest summaries");
            return StatusCode(500);
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