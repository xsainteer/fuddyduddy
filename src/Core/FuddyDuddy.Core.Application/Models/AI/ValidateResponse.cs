using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Application.Models.AI;

public class ValidationResponse : IAiModelResponse
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;
}