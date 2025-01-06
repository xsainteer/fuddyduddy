using System.Net.Http.Json;
using System.Text.Json;
using FuddyDuddy.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Domain.Entities;
using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Services;

public class SummaryValidationService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SummaryValidationService> _logger;

    public SummaryValidationService(
        INewsSummaryRepository summaryRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<SummaryValidationService> logger)
    {
        _summaryRepository = summaryRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ValidateNewSummariesAsync(CancellationToken cancellationToken = default)
    {
        var newSummaries = await _summaryRepository.GetByStateAsync(NewsSummaryState.Created, cancellationToken);

        foreach (var summary in newSummaries)
        {
            try
            {
                var validation = await ValidateSummaryAsync(summary, cancellationToken);
                
                if (validation.IsValid)
                {
                    summary.Validate(validation.Reason);
                    _logger.LogInformation("Summary {Id} validated successfully: {Reason}", summary.Id, validation.Reason);
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
        using var httpClient = _httpClientFactory.CreateClient();
        var request = new
        {
            model = "owl/t-lite",
            messages = new[]
            {
                new
                {
                    role = "system", 
                    content = @"Ты - ассистент по валидации. Сравни оригинальный заголовок статьи с заголовком и содержанием краткого изложения.
                               Верни JSON-ответ, указывающий, совпадают ли они семантически (isValid) и почему (reason).
                               Учитывай: 1) Релевантность заголовка 2) Точность краткого изложения 3) Общее качество.
                               Если isValid=false, в reason укажи причину отклонения. Если isValid=true, в reason укажи подтверждение качества.
                               Если isValid=false, можешь семантику ссылки на статью (она может идти в транслитерации заголовка)."
                },
                new
                {
                    role = "user",
                    content = $"Оригинальный заголовок: {summary.NewsArticle.Title}\nОригинальная ссылка: {summary.NewsArticle.Url}\nЗаголовок краткого изложения: {summary.Title}\nСодержание краткого изложения: {summary.Article}"
                }
            },
            format = new
            {
                type = "object",
                properties = new
                {
                    isValid = new { type = "boolean" },
                    reason = new { type = "string" }
                },
                required = new[] { "isValid", "reason" }
            },
            stream = false,
            options = new { temperature = 0.3 }
        };

        using var response = await httpClient.PostAsJsonAsync("http://localhost:11434/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        if (result?.Message?.Content == null)
            return new ValidationResponse { IsValid = false, Reason = "Failed to get validation response" };

        try
        {
            var validation = JsonSerializer.Deserialize<ValidationResponse>(result.Message.Content);
            return validation ?? new ValidationResponse { IsValid = false, Reason = "Failed to parse validation response" };
        }
        catch (Exception ex)
        {
            return new ValidationResponse { IsValid = false, Reason = $"Validation error: {ex.Message}" };
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

    private class ValidationResponse
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
} 