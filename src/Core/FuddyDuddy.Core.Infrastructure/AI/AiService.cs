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
    private const string GEMINI_PREFIX = "gemini";
    private const string GEMINI2_PREFIX = "gemini2";
    private readonly ILogger<AiService> _logger;
    private readonly Dictionary<ModelType, IAiClient> _geminiClients;
    private readonly Dictionary<ModelType, IAiClient> _gemini2Clients;
    private readonly Dictionary<ModelType, IAiClient> _ollamaClients;
    private readonly IOptions<AiModels> _aiModelsOptions;

    public AiService(IServiceProvider serviceProvider, ILogger<AiService> logger, IOptions<AiModels> aiModelsOptions)
    {
        _logger = logger;
        _aiModelsOptions = aiModelsOptions;
        _geminiClients = new Dictionary<ModelType, IAiClient>
        {
            { ModelType.Light, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini, ModelType.Light, GEMINI_PREFIX) },
            { ModelType.Pro, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini, ModelType.Pro, GEMINI_PREFIX) }
        };

        _gemini2Clients = new Dictionary<ModelType, IAiClient>
        {
            { ModelType.Light, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini2, ModelType.Light, GEMINI2_PREFIX) },
            { ModelType.Pro, ActivatorUtilities.CreateInstance<GeminiClient>(serviceProvider, aiModelsOptions.Value.Gemini2, ModelType.Pro, GEMINI2_PREFIX) }
        };

        _ollamaClients = new Dictionary<ModelType, IAiClient>
        {
            { ModelType.SuperLight, ActivatorUtilities.CreateInstance<OllamaClient>(serviceProvider, aiModelsOptions.Value.Ollama, ModelType.SuperLight) },
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
        if (!_aiModelsOptions.Value.ResponseMappings.TryGetValue(typeof(T).Name, out var mapping))
        {
            throw new ArgumentException($"No mapping found for {typeof(T).Name}");
        }

        var modelDictionary = mapping.Model switch
        {
            "Gemini" => _geminiClients,
            "Gemini2" => _gemini2Clients,
            "Ollama" => _ollamaClients,
            _ => throw new ArgumentException($"Invalid model type for {typeof(T).Name}")
        };

        if (!modelDictionary.TryGetValue(mapping.ModelType, out var client))
        {
            throw new ArgumentException($"Invalid model type for {typeof(T).Name}");
        }

        return client;
    }
}
