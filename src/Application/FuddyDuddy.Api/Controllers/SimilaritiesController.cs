using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class SimilaritiesController : Controller
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<SimilaritiesController> _logger;
    private readonly ISimilarityService _similarityService;

    public SimilaritiesController(ICacheService cacheService, ILogger<SimilaritiesController> logger, ISimilarityService similarityService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _similarityService = similarityService;
    }

    [HttpGet("/{summaryId}/allSimilarities")]
    public async Task<IActionResult> GetDbSimilaritiesBySummaryId(string summaryId,[FromQuery] int offset = 0, [FromQuery] int limit = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Guid.TryParse(summaryId, out Guid id))
            {
                var references = await _similarityService.GetDbSimilaritiesBySummaryId(id, offset, limit, cancellationToken);
                return Ok(references);
            }
            return NotFound(new {message = "Summary not found"});
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}