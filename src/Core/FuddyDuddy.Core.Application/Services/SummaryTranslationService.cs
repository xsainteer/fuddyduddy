using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Application.Models.AI;

namespace FuddyDuddy.Core.Application.Services;

public interface ISummaryTranslationService
{
    Task TranslatePendingAsync(Language targetLanguage, CancellationToken cancellationToken = default);
    Task<NewsSummary?> TranslateSummaryAsync(Guid summaryId, Language targetLanguage, CancellationToken cancellationToken = default);
}

internal class SummaryTranslationService : ISummaryTranslationService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SummaryTranslationService> _logger;
    private readonly IAiService _aiService;

    public SummaryTranslationService(
        INewsSummaryRepository summaryRepository,
        ICacheService cacheService,
        ILogger<SummaryTranslationService> logger,
        IAiService aiService)
    {
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _logger = logger;
        _aiService = aiService;
    }

    public async Task TranslatePendingAsync(Language targetLanguage, CancellationToken cancellationToken = default)
    {
        var summaries = (await _summaryRepository.GetByStateAsync(
                [NewsSummaryState.Validated, NewsSummaryState.Digested], 
                dateStart: DateTimeOffset.UtcNow.AddHours(-1), 
                cancellationToken: cancellationToken))
            .OrderBy(s => s.GeneratedAt)
            .ToArray();
        foreach (var summary in summaries)
        {
            await TranslateSummaryAsync(summary.Id, targetLanguage, cancellationToken);
        }
    }

    public async Task<NewsSummary?> TranslateSummaryAsync(Guid summaryId, Language targetLanguage, CancellationToken cancellationToken = default)
    {
        var summary = await _summaryRepository.GetByIdAsync(summaryId, cancellationToken);
        if (summary == null)
        {
            _logger.LogWarning("Summary {Id} not found", summaryId);
            return null;
        }

        if (summary.Language == targetLanguage)
        {
            _logger.LogInformation("Summary {Id} is already in {Language}", summaryId, targetLanguage);
            return null;
        }

        try
        {
            var existingSummaries = await _summaryRepository.GetByNewsArticleIdAsync(summary.NewsArticleId, cancellationToken);
            if (existingSummaries.Any(s => s.Language == targetLanguage))
            {
                _logger.LogInformation("Summary {Id} already exists in {Language}. Skipping translation.", summaryId, targetLanguage);
                return null;
            }

            var translation = await GetTranslationAsync(summary, targetLanguage, cancellationToken);
            if (translation == null)
            {
                _logger.LogError("Failed to get translation for summary {Id}", summaryId);
                return null;
            }

            var translatedSummary = new NewsSummary(
                summary.NewsArticleId,
                translation.Title,
                translation.Article,
                summary.CategoryId,
                targetLanguage);

            translatedSummary.Validate(); // TODO: add validation step for translated summaries

            await _summaryRepository.AddAsync(translatedSummary, cancellationToken);

            await _cacheService.AddSummaryAsync(translatedSummary.Id, cancellationToken);

            _logger.LogInformation("Created translation {TranslatedId} for summary {Id} in {Language}", 
                translatedSummary.Id, summaryId, targetLanguage);

            return translatedSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error translating summary {Id} to {Language}", summaryId, targetLanguage);
            return null;
        }
    }

    private async Task<TranslationResponse?> GetTranslationAsync(NewsSummary summary, Language targetLanguage, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = $@"You are a professional translator. Translate the following news title and summary from {summary.Language.GetDescription()} to {targetLanguage.GetDescription()}.
                                    Keep the translation accurate but natural in the target language.
                                    Respond in JSON format with 'title' and 'article' fields containing the translations.
                                    Maintain the same tone and style as the original text.";

            var userPrompt = $"Title: {summary.Title}\nArticle: {summary.Article}";

            var response = await _aiService.GenerateStructuredResponseAsync<TranslationResponse>(
                systemPrompt,
                userPrompt,
                new TranslationResponse(),
                cancellationToken
            );

            if (response == null)
                return null;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate summary {Id} to {Language}. Error: {Error}", summary.Id, targetLanguage, ex.Message);
            return null;
        }
    }
} 