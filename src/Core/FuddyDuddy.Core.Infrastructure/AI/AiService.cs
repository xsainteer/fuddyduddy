using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Models.AI;
using ModelType = FuddyDuddy.Core.Infrastructure.Configuration.AiModels.Type;

namespace FuddyDuddy.Core.Infrastructure.AI;

internal class AiService : IAiService
{
    private readonly ILogger<AiService> _logger;
    private readonly Dictionary<ModelType, IAiClient> _geminiClients;
    private readonly Dictionary<ModelType, IAiClient> _gemini2Clients;
    private readonly Dictionary<ModelType, IAiClient> _ollamaClients;
    public AiService(IServiceProvider serviceProvider, ILogger<AiService> logger, IOptions<AiModels> aiModelsOptions)
    {
        _logger = logger;

        _geminiClients = new Dictionary<ModelType, IAiClient>
        {
            { ModelType.Light, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini, ModelType.Light) },
            { ModelType.Pro, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini, ModelType.Pro) }
        };

        _gemini2Clients = new Dictionary<ModelType, IAiClient>
        {
            { ModelType.Light, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini2, ModelType.Light) },
            { ModelType.Pro, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini2, ModelType.Pro) }
        };

        _ollamaClients = new Dictionary<ModelType, IAiClient>
        {
            { ModelType.Light, ActivatorUtilities.CreateInstance<OllamaClient>(serviceProvider, aiModelsOptions.Value.Ollama, ModelType.Light) },
            { ModelType.Pro, ActivatorUtilities.CreateInstance<OllamaClient>(serviceProvider, aiModelsOptions.Value.Ollama, ModelType.Pro) }
        };
    }

    public async Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        T sample,
        CancellationToken cancellationToken = default) where T : IAiModelResponse
    {
        var client = GetModelClient<T>();
        _logger.LogInformation("Generating structured response for {ModelType} with client {Client}", typeof(T).Name, client.GetType().Name);
        return await client.GenerateStructuredResponseAsync(systemPrompt, userInput, sample, cancellationToken);
    }

    private IAiClient GetModelClient<T>() where T : IAiModelResponse
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        return typeof(T).Name switch
        {
            nameof(TweetCreationResponse) => isDevelopment ? _ollamaClients[ModelType.Light] : _geminiClients[ModelType.Pro],
            nameof(DigestResponse) => isDevelopment ? _ollamaClients[ModelType.Light] : _geminiClients[ModelType.Light],
            nameof(SummaryResponse) => _ollamaClients[ModelType.Light],
            nameof(ValidationResponse) => _ollamaClients[ModelType.Light],
            nameof(TranslationResponse) => _ollamaClients[ModelType.Light],
            nameof(SimilarityResponse) => isDevelopment ? _ollamaClients[ModelType.Light] : _gemini2Clients[ModelType.Light],
            _ => throw new ArgumentException($"Invalid model type for {typeof(T).Name}")
        };
    }
}