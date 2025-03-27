using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class SimilaritiesController : Controller
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<SimilaritiesController> _logger;
    private readonly ISimilarRepository _similarRepository;

    public SimilaritiesController(ICacheService cacheService, ILogger<SimilaritiesController> logger, ISimilarRepository similarRepository)
    {
        _cacheService = cacheService;
        _logger = logger;
        _similarRepository = similarRepository;
    }

    [HttpGet("/{summaryId}/allSimilarities")]
    public async Task<IActionResult> GetDbSimilaritiesBySummaryId(string summaryId,[FromQuery] int offset = 0, [FromQuery] int limit = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Guid.TryParse(summaryId, out Guid id))
            {
                var similars = await _similarRepository.GetBySummaryIdAsync(id, cancellationToken);
                if (similars.Any())
                {
                    var references = similars
                        .SelectMany(s => s.References)
                        //mapping it to cache DTO so web-react will accept it (they have the same structure)
                        .OrderByDescending(r => r.NewsSummary.GeneratedAt)
                        .Skip(offset)
                        .Take(limit)
                        .Select(CachedSimilarReferenceBaseDto.FromSimilarReference)
                        .ToList();
                    return Ok(references);
                }
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