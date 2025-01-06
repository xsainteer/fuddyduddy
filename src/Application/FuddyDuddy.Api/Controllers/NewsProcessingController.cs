using FuddyDuddy.Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsProcessingController : ControllerBase
{
    private readonly NewsProcessingService _newsProcessingService;
    private readonly ILogger<NewsProcessingController> _logger;

    public NewsProcessingController(
        NewsProcessingService newsProcessingService,
        ILogger<NewsProcessingController> logger)
    {
        _newsProcessingService = newsProcessingService;
        _logger = logger;
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
} 