using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FuddyDuddy.Core.Infrastructure.AI;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly string _apiKey;

    public GeminiAiService(string apiKey, ILogger<GeminiAiService> logger)
    {
        _apiKey = apiKey;
        _logger = logger;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FuddyDuddy/1.0 (News Digest Application)");
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\n" + userInput }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json"
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var requestJson = JsonSerializer.Serialize(request, jsonOptions);
            _logger.LogInformation("Sending request: {Request}", requestJson);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, 
                $"v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}");
            httpRequest.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request uri: {Request}", httpRequest.RequestUri);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
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