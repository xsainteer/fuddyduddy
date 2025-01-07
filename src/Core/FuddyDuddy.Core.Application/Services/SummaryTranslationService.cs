using System.Net.Http.Json;
using System.Text.Json;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Services;

public class SummaryTranslationService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SummaryTranslationService> _logger;

    public SummaryTranslationService(
        INewsSummaryRepository summaryRepository,
        ICacheService cacheService,
        IHttpClientFactory httpClientFactory,
        ILogger<SummaryTranslationService> logger)
    {
        _summaryRepository = summaryRepository;
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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
            _logger.LogWarning("Summary {Id} is already in {Language}", summaryId, targetLanguage);
            return null;
        }

        try
        {
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

            await _summaryRepository.AddAsync(translatedSummary, cancellationToken);
            await _cacheService.AddSummaryAsync(translatedSummary, cancellationToken);

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
        using var httpClient = _httpClientFactory.CreateClient();
        var request = new
        {
            model = "owl/t-lite",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = $@"You are a professional translator. Translate the following news title and summary from {summary.Language.GetDescription()} to {targetLanguage.GetDescription()}.
                                Keep the translation accurate but natural in the target language.
                                Respond in JSON format with 'title' and 'article' fields containing the translations.
                                Maintain the same tone and style as the original text."
                },
                new
                {
                    role = "user",
                    content = $"Title: {summary.Title}\nArticle: {summary.Article}"
                }
            },
            format = new
            {
                type = "object",
                properties = new
                {
                    title = new { type = "string" },
                    article = new { type = "string" }
                },
                required = new[] { "title", "article" }
            },
            stream = false,
            options = new
            {
                temperature = 0.3,
                num_ctx = 8192
            }
        };

        var response = await httpClient.PostAsJsonAsync("http://localhost:11434/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        if (result?.Message?.Content == null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<TranslationResponse>(result.Message.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse translation response: {Response}", result.Message.Content);
            return null;
        }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class TranslationResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("article")]
        public string Article { get; set; } = string.Empty;
    }
} 