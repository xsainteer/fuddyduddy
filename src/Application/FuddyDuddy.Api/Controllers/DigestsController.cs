using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Application.Models;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DigestsController : ControllerBase
{
    private readonly IDigestCookService _digestCookService;
    private readonly IDigestRepository _digestRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DigestsController> _logger;

    public DigestsController(
        IDigestCookService digestCookService,
        IDigestRepository digestRepository,
        ICacheService cacheService,
        ILogger<DigestsController> logger)
    {
        _digestCookService = digestCookService;
        _digestRepository = digestRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestDigests(
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] Language? language = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parsedLanguage = language ?? Language.RU;
            
            // Try to get from cache first
            var digests = await _cacheService.GetLatestDigestsAsync<CachedDigestDto>(
                page * pageSize,
                pageSize,
                parsedLanguage,
                cancellationToken);

            // If not in cache, get from database and cache them
            if (!digests.Any())
            {
                var dbDigests = await _digestRepository.GetLatestAsync(pageSize, parsedLanguage, page * pageSize, cancellationToken);
                var digestDtos = dbDigests.Select(d => CachedDigestDto.FromDigest(d)).ToList();

                // Cache each digest
                foreach (var digest in dbDigests)
                {
                    await _cacheService.AddDigestAsync(CachedDigestDto.FromDigest(digest), cancellationToken);
                }

                return Ok(digestDtos);
            }

            return Ok(digests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting digests. Page: {Page}", page);
            return StatusCode(500, new { message = "An error occurred while fetching digests" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDigestById(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get from cache first
            var digest = await _cacheService.GetDigestByIdAsync<CachedDigestDto>(id, cancellationToken);
            
            if (digest == null)
            {
                // Try to get from database if not in cache
                if (Guid.TryParse(id, out var digestId))
                {
                    var dbDigest = await _digestRepository.GetByIdAsync(digestId, cancellationToken);
                    if (dbDigest != null)
                    {
                        digest = CachedDigestDto.FromDigest(dbDigest);
                        
                        // Cache asynchronously without waiting
                        _ = _cacheService.CacheDigestDtoAsync(id, digest, cancellationToken);
                        
                        return Ok(digest);
                    }
                }
                
                return NotFound(new { message = "Digest not found" });
            }

            return Ok(digest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting digest by ID. Id: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the digest" });
        }
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDigest(
        [FromQuery] string language = "RU",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Language>(language, true, out var parsedLanguage))
            {
                return BadRequest(new { message = "Invalid language" });
            }

            await _digestCookService.GenerateDigestAsync(parsedLanguage, cancellationToken);
            return Ok(new { message = "Digest generation started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating digest for language {Language}", language);
            return StatusCode(500, new { message = "An error occurred while generating the digest" });
        }
    }
}