using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Models.AI;

public class TranslationResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("article")]
    public string Article { get; set; } = string.Empty;
}