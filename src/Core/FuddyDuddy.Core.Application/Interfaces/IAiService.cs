namespace FuddyDuddy.Core.Application.Interfaces;

public interface IAiService
{
    Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        CancellationToken cancellationToken = default) where T : class;
} 