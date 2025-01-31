using System.Net.Http.Json;
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
    private readonly AiModels.ModelOptions.Characteristic _modelOptions;

    public OllamaEmbeddingService(
        IHttpClientFactory httpClientFactory,
        IOptions<AiModels> aiModelsOptions,
        ILogger<OllamaEmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Get the embedding model name from configuration
        _modelOptions = aiModelsOptions.Value.Ollama.Models
            .First(m => m.Type == AiModels.Type.Embedding);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(HttpClientConstants.OLLAMA);
        
        var request = new OllamaEmbeddingRequest
        {
            Model = _modelOptions.Name,
            Prompt = text,
            Options = new OllamaEmbeddingOptions
            {
                NumCtx = _modelOptions.MaxTokens,
                Temperature = _modelOptions.Temperature
            }
        };

        try
        {
            using var response = await client.PostAsJsonAsync("api/embed", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken);
            if (result?.Embeddings == null || result.Embeddings.Length == 0 || result.Embeddings[0] == null || result.Embeddings[0].Length == 0)
            {
                throw new Exception("Embedding response was null or empty");
            }

            _logger.LogInformation("Embedding {Embedding} generated for text: {Text}", string.Join(",", result.Embeddings[0]), text);

            return result.Embeddings[0];
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

        [JsonPropertyName("input")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public OllamaEmbeddingOptions Options { get; set; } = new();
    }

    private class OllamaEmbeddingOptions
    {
        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
    }

    private class OllamaEmbeddingResponse
    {

        [JsonPropertyName("embeddings")]
        public float[][] Embeddings { get; set; } = Array.Empty<float[]>();

    }
} 