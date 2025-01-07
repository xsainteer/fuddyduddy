using Microsoft.AspNetCore.Mvc;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiltersController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly INewsSourceRepository _newsSourceRepository;
    private readonly ILogger<FiltersController> _logger;

    public FiltersController(
        ICategoryRepository categoryRepository,
        INewsSourceRepository newsSourceRepository,
        ILogger<FiltersController> logger)
    {
        _categoryRepository = categoryRepository;
        _newsSourceRepository = newsSourceRepository;
        _logger = logger;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            return Ok(categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                local = c.Local
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "An error occurred while fetching categories" });
        }
    }

    [HttpGet("sources")]
    public async Task<IActionResult> GetSources(CancellationToken cancellationToken = default)
    {
        try
        {
            var sources = await _newsSourceRepository.GetActiveSourcesAsync(cancellationToken);
            return Ok(sources.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                domain = s.Domain
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting news sources");
            return StatusCode(500, new { message = "An error occurred while fetching news sources" });
        }
    }

    [HttpGet("languages")]
    public IActionResult GetLanguages()
    {
        try
        {
            var languages = Enum.GetValues<Language>().Select(l => new
            {
                id = l.ToString().ToLower(),
                name = l.GetDescription(),
                local = l.GetDescriptionInLocal()
            });
            return Ok(languages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting languages");
            return StatusCode(500, new { message = "An error occurred while fetching languages" });
        }
    }
} 