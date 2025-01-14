using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallbackController : ControllerBase
{
    private readonly ITwitterAuthService _twitterAuthService;
    private readonly ILogger<CallbackController> _logger;

    public CallbackController(
        ITwitterAuthService twitterAuthService,
        ILogger<CallbackController> logger)
    {
        _twitterAuthService = twitterAuthService;
        _logger = logger;
    }

    [HttpGet("twitter")]
    public async Task<IActionResult> TwitterCallback(string code, string state, CancellationToken cancellationToken = default)
    {
        try
        {
            await _twitterAuthService.HandleCallbackAsync(code, state, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Twitter callback");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}