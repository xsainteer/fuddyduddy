using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Application.Models.AI;

namespace FuddyDuddy.Core.Application.Services;

public class SummaryValidationService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SummaryValidationService> _logger;
    private readonly IOllamaService _ollamaService;

    public SummaryValidationService(
        INewsSummaryRepository summaryRepository,
        ICacheService cacheService,
        ILogger<SummaryValidationService> logger,
        IOllamaService ollamaService)
    {
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _ollamaService = ollamaService;
    }

    public async Task ValidateNewSummariesAsync(CancellationToken cancellationToken = default)
    {
        var newSummaries = (await _summaryRepository.GetByStateAsync([NewsSummaryState.Created], cancellationToken: cancellationToken))
            .OrderBy(s => s.GeneratedAt);

        foreach (var summary in newSummaries)
        {
            try
            {
                var validation = await ValidateSummaryAsync(summary, cancellationToken);
                
                if (validation.IsValid)
                {
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

    private async Task<ValidationResponse> ValidateSummaryAsync(NewsSummary summary, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = @"Ты - ассистент по валидации. Сравни оригинальный заголовок статьи с заголовком и содержанием краткого изложения.
                                Верни JSON-ответ, указывающий, совпадают ли они семантически (isValid) и почему (reason).
                                Учитывай: 1) Релевантность заголовка 2) Точность краткого изложения 3) Общее качество.
                                Если isValid=false, в reason укажи причину отклонения. Если isValid=true, в reason укажи подтверждение качества.
                                Если isValid=false, можешь семантику ссылки на статью (она может идти в транслитерации заголовка).";
            
            var userInput = @$"Оригинальный заголовок: {summary.NewsArticle.Title}
                                Оригинальная ссылка: {summary.NewsArticle.Url}
                                Заголовок краткого изложения: {summary.Title}
                                Содержание краткого изложения: {summary.Article}";

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