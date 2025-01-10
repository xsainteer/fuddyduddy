using System.Text.Json;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Infrastructure.Configuration;
using FuddyDuddy.Core.Application;

namespace FuddyDuddy.Core.Infrastructure.AI;

public class GeminiAiService : IGeminiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly IOptions<GeminiOptions> _geminiOptions;

    public GeminiAiService(IOptions<GeminiOptions> geminiOptions, ILogger<GeminiAiService> logger, IHttpClientFactory httpClientFactory)
    {
        _geminiOptions = geminiOptions;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        T sample,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(Constants.GEMINI);
            var sampleJson = JsonSerializer.Serialize(sample, IAiService.SampleJsonOptions);
            _logger.LogInformation("Sample digest: {Sample}", sampleJson);
            var sampleText = $"Format your response as a JSON object with the following structure:\n{sampleJson}";

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\n" + sampleText + "\n\n" + userInput }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json"
                }
            };

            var requestJson = JsonSerializer.Serialize(request, IAiService.RequestJsonOptions);
            _logger.LogInformation("Sending request: {Request}", requestJson);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"v1beta/models/{_geminiOptions.Value.Model}:generateContent?key={_geminiOptions.Value.ApiKey}");
            httpRequest.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request uri: {Request}", httpRequest.RequestUri);

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Raw Gemini response: {Response}", responseContent);
            response.EnsureSuccessStatusCode();

            // var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            // _logger.LogInformation("Raw Gemini response: {Response}", responseContent);

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            var content = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogError("Empty response from Gemini API");
                return null;
            }

            _logger.LogInformation("Parsed content: {Content}", content);
            return JsonSerializer.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response from Gemini API");
            return null;
        }
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
} 