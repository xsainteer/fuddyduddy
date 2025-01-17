using System.Text.Json;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Infrastructure.Configuration;
using FuddyDuddy.Core.Application.Constants;

namespace FuddyDuddy.Core.Infrastructure.AI;

internal class GeminiClient : IAiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiClient> _logger;
    private readonly AiModels.ModelOptions _geminiOptions;
    private readonly string _model;
    public GeminiClient(AiModels.ModelOptions geminiOptions, AiModels.Type type, ILogger<GeminiClient> logger, IHttpClientFactory httpClientFactory)
    {
        _geminiOptions = geminiOptions;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _model = geminiOptions.Models.First(m => m.Type == type).Name;
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        T sample,
        CancellationToken cancellationToken = default) where T : IAiModelResponse
    {
        string content = string.Empty;
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.GEMINI);
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
                            new { text = @$"{systemPrompt}

{sampleText}

{userInput}" }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json"
                }
            };

            var requestJsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var requestJson = JsonSerializer.Serialize(request, requestJsonOptions);
            _logger.LogInformation("Sending request: {Request}", requestJson);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"v1beta/models/{_model}:generateContent?key={_geminiOptions.ApiKey}");
            httpRequest.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request uri: {Request}", httpRequest.RequestUri);

            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Raw Gemini response: {Response}", responseContent);
            response.EnsureSuccessStatusCode();

            // var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            // _logger.LogInformation("Raw Gemini response: {Response}", responseContent);

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            content = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogError("Empty response from Gemini API");
                return default;
            }

            _logger.LogInformation("Parsed content: {Content}", content);
            return JsonSerializer.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response from Gemini API, Content: {Content}", content);
            return default;
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