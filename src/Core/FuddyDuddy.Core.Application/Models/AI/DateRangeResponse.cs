using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Application.Models.AI;

public class DateRangeResponse : IAiModelResponse
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }
}
