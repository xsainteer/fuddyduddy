using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DigestsController : ControllerBase
{
    private readonly DigestCookService _digestCookService;
    private readonly IDigestRepository _digestRepository;
    private readonly ILogger<DigestsController> _logger;

    public DigestsController(
        DigestCookService digestCookService,
        IDigestRepository digestRepository,
        ILogger<DigestsController> logger)
    {
        _digestCookService = digestCookService;
        _digestRepository = digestRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestDigests(
        [FromQuery] string language = "RU",
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Language>(language, true, out var parsedLanguage))
            {
                return BadRequest(new { message = "Invalid language" });
            }

            var digests = await _digestRepository.GetLatestAsync(count, parsedLanguage, cancellationToken);
            return Ok(digests.Select(d => new DigestDto
            {
                Id = d.Id,
                Title = d.Title,
                Content = d.Content,
                GeneratedAt = d.GeneratedAt,
                PeriodStart = d.PeriodStart,
                PeriodEnd = d.PeriodEnd,
                References = d.References.Select(r => new DigestReferenceDto
                {
                    Title = r.Title,
                    Url = r.Url,
                    Reason = r.Reason
                }).ToList()
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest digests");
            return StatusCode(500, new { message = "An error occurred while getting digests" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDigestById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var digest = await _digestRepository.GetByIdAsync(id, cancellationToken);
            if (digest == null)
                return NotFound();

            return Ok(new DigestDto
            {
                Id = digest.Id,
                Title = digest.Title,
                Content = digest.Content,
                GeneratedAt = digest.GeneratedAt,
                PeriodStart = digest.PeriodStart,
                PeriodEnd = digest.PeriodEnd,
                References = digest.References.Select(r => new DigestReferenceDto
                {
                    Title = r.Title,
                    Url = r.Url,
                    Reason = r.Reason
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting digest by ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting the digest" });
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

public class DigestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    public List<DigestReferenceDto> References { get; set; } = new();
}

public class DigestReferenceDto
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
} 