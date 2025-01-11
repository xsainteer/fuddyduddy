using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Models.AI;

public class ValidationResponse
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public int? Category { get; set; }
}