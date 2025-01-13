using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Application.Models.AI;

public class TweetCreationResponse : IAiModelResponse
{
    [JsonPropertyName("tweet")]
    public string Tweet { get; set; }
}