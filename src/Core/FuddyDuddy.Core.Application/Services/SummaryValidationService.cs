using System.Linq;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Models.Broker;

namespace FuddyDuddy.Core.Application.Services;

public interface ISummaryValidationService
{
    Task ValidateNewSummariesAsync(CancellationToken cancellationToken = default);
    Task<ValidationResponse> ValidateSummaryAsync(NewsSummary summary, string categoryPrompt, CancellationToken cancellationToken);
}

internal class SummaryValidationService : ISummaryValidationService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SummaryValidationService> _logger;
    private readonly IAiService _aiService;
    private readonly ICategoryRepository _categoryRepository;
    public SummaryValidationService(
        INewsSummaryRepository summaryRepository,
        ICategoryRepository categoryRepository,
        ICacheService cacheService,
        ILogger<SummaryValidationService> logger,
        IAiService aiService)
    {
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _aiService = aiService;
        _categoryRepository = categoryRepository;
    }

    public async Task ValidateNewSummariesAsync(CancellationToken cancellationToken = default)
    {
        var newSummaries = (await _summaryRepository.GetByStateAsync([NewsSummaryState.Created], cancellationToken: cancellationToken))
            .OrderBy(s => s.GeneratedAt);

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken)).ToDictionary(c=>c.Local, c=>c);
        var categoryPrompt = string.Join("\n", categories.Select(c => $"{c.Key} ({c.Value.KeywordsLocal})"));

        foreach (var summary in newSummaries)
        {
            try
            {
                var validation = await ValidateSummaryAsync(summary, categoryPrompt, cancellationToken);
                
                if (validation.IsValid)
                {
                    if (validation.Topic != summary.Category.Local && categories.TryGetValue(validation.Topic, out var newCategory))
                    {
                        _logger.LogInformation("Category {OldCategoryId} for summary {SummaryId} updated to {NewCategoryId}", 
                            summary.Category.Id, summary.Id, newCategory.Id);
                        summary.UpdateCategory(newCategory.Id);
                    }

                    summary.Validate(validation.Reason);
                    _logger.LogInformation("Summary {Id} validated successfully: {Reason}", summary.Id, validation.Reason);
                }
                else
                {
                    summary.Discard(validation.Reason);
                    _logger.LogWarning("Summary {Id} discarded: {Reason}", summary.Id, validation.Reason);
                }

                await _summaryRepository.UpdateAsync(summary, cancellationToken);

                if (summary.State == NewsSummaryState.Validated)
                {
                    // Add to cache only if validation passed
                    await _cacheService.AddSummaryAsync(summary, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating summary {Id}", summary.Id);
            }
        }
    }

    public async Task<ValidationResponse> ValidateSummaryAsync(NewsSummary summary, string categoryPrompt, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = @$"Ты - ассистент главного редактора новостной колонки. Сравни оригинальный заголовок статьи с заголовком и содержанием краткого изложения.
                                Верни JSON-ответ, указывающий, совпадают ли они семантически (isValid), почему (reason) и определи тематика (topic).
                                Учитывай: 1) Релевантность заголовка 2) Точность краткого изложения 3) Общее качество.
                                Если isValid=false, в reason укажи причину отклонения. Если isValid=true, в reason укажи подтверждение качества.
                                
                                Тематику (topic) определи из списка (ключевые слова в скобках), верни только название тематики:
                                {categoryPrompt}";
            
            var userInput = @$"Оригинальный заголовок: {summary.NewsArticle.Title}
                                Заголовок краткого изложения: {summary.Title}
                                Содержание краткого изложения: {summary.Article}";

            var response = await _aiService.GenerateStructuredResponseAsync<ValidationResponse>(
                systemPrompt,
                userInput,
                new ValidationResponse(),
                cancellationToken
            );
            
            if (response == null)
                return new ValidationResponse { IsValid = false, Reason = "Failed to get validation response" };

            return response;
        }
        catch (Exception ex)
        {
            return new ValidationResponse { IsValid = false, Reason = $"Validation error: {ex.Message}" };
        }
    }
} 