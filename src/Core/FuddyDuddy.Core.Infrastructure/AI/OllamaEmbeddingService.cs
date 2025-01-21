using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FuddyDuddy.Core.Infrastructure.AI;

internal sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OllamaEmbeddingService> _logger;
    private readonly string _modelName;

    public OllamaEmbeddingService(
        IHttpClientFactory httpClientFactory,
        IOptions<AiModels> aiModelsOptions,
        ILogger<OllamaEmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Get the embedding model name from configuration
        _modelName = aiModelsOptions.Value.Ollama.Models
            .First(m => m.Type == AiModels.Type.Embedding).Name;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(HttpClientConstants.OLLAMA);
        
        var request = new OllamaEmbeddingRequest
        {
            Model = _modelName,
            Prompt = text
        };

        try
        {
            var response = await client.PostAsJsonAsync("api/embeddings", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken);
            if (result?.Embedding == null)
            {
                throw new Exception("Embedding response was null");
            }

            _logger.LogInformation("Embedding {Embedding} generated for text: {Text}", string.Join(",", result.Embedding), text);

            return result.Embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text);
            throw;
        }
    }

    public async Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var tasks = texts.Select(text => GenerateEmbeddingAsync(text, cancellationToken));
        return await Task.WhenAll(tasks);
    }

    private class OllamaEmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;
    }

    private class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
} 