using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Application.Models.AI;

public class DigestResponse : IAiModelResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("references")]
    public List<ReferenceResponse> References { get; set; } = new();
}

public class ReferenceResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}