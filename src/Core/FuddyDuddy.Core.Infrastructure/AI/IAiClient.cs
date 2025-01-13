using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Infrastructure.AI;

internal interface IAiClient
{
    Task<T?> GenerateStructuredResponseAsync<T>(string systemPrompt, string userInput, T sample, CancellationToken cancellationToken = default) where T : IAiModelResponse;
}