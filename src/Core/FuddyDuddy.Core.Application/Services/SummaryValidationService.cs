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
}

internal class SummaryValidationService : ISummaryValidationService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SummaryValidationService> _logger;
    private readonly IOllamaService _ollamaService;
    private readonly ICategoryRepository _categoryRepository;
    public SummaryValidationService(
        INewsSummaryRepository summaryRepository,
        ICategoryRepository categoryRepository,
        ICacheService cacheService,
        ILogger<SummaryValidationService> logger,
        IOllamaService ollamaService)
    {
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _ollamaService = ollamaService;
        _categoryRepository = categoryRepository;
    }

    public async Task ValidateNewSummariesAsync(CancellationToken cancellationToken = default)
    {
        var newSummaries = (await _summaryRepository.GetByStateAsync([NewsSummaryState.Created], cancellationToken: cancellationToken))
            .OrderBy(s => s.GeneratedAt);

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var categoryPrompt = string.Join("\n", categories.Select(c => $"{c.Id}. {c.Local}"));

        foreach (var summary in newSummaries)
        {
            try
            {
                var validation = await ValidateSummaryAsync(summary, categoryPrompt, cancellationToken);
                
                if (validation.IsValid)
                {
                    if (validation.Category.HasValue)
                    {
                        if (await _categoryRepository.IsValidCategoryAsync(validation.Category.Value, cancellationToken) && validation.Category.Value != summary.Category.Id)
                        {
                            _logger.LogInformation("LLM suggested category {OldCategoryId} for summary {SummaryId}, updating to {NewCategoryId}", 
                                summary.Category.Id, summary.Id, validation.Category.Value);
                            summary.UpdateCategory(validation.Category.Value);
                        }
                        else
                        {
                            _logger.LogWarning("Invalid category {CategoryId} for summary {SummaryId}", validation.Category.Value, summary.Id);
                        }
                    }

                    summary.Validate(validation.Reason);
                    _logger.LogInformation("Summary {Id} validated successfully: {Reason}", summary.Id, validation.Reason);
                    
                    // Add to cache only if validation passed
                    await _cacheService.AddSummaryAsync(summary, cancellationToken);
                }
                else
                {
                    summary.Discard(validation.Reason);
                    _logger.LogWarning("Summary {Id} discarded: {Reason}", summary.Id, validation.Reason);
                }

                await _summaryRepository.UpdateAsync(summary, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating summary {Id}", summary.Id);
            }
        }
    }

    private async Task<ValidationResponse> ValidateSummaryAsync(NewsSummary summary, string categoryPrompt, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = @$"Ты - ассистент по валидации. Сравни оригинальный заголовок статьи с заголовком и содержанием краткого изложения.
                                Верни JSON-ответ, указывающий, совпадают ли они семантически (isValid) и почему (reason).
                                Учитывай: 1) Релевантность заголовка 2) Точность краткого изложения 3) Общее качество.
                                Если isValid=false, в reason укажи причину отклонения. Если isValid=true, в reason укажи подтверждение качества.
                                Если isValid=false, можешь семантику ссылки на статью (она может идти в транслитерации заголовка).

                                Также оцени категорию, если ты считаешь, что она не соответствует содержанию статьи, предложи правильную из списка:
                                {categoryPrompt}
                                
                                И скажи почему ты так решил (дополни reason).
                                Для category верни только ID категории (число от 1 до 16).";
            
            var userInput = @$"Оригинальный заголовок: {summary.NewsArticle.Title}
                                Оригинальная ссылка: {summary.NewsArticle.Url}
                                Заголовок краткого изложения: {summary.Title}
                                Содержание краткого изложения: {summary.Article}
                                Категория: {summary.Category.Local}";

            var response = await _ollamaService.GenerateStructuredResponseAsync<ValidationResponse>(
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