using System.Text.Json;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface IAiService
{
    public static JsonSerializerOptions SampleJsonOptions => new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static JsonSerializerOptions RequestJsonOptions => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Task<T?> GenerateStructuredResponseAsync<T>(
        string systemPrompt,
        string userInput,
        T sample,
        CancellationToken cancellationToken = default) where T : class;
}