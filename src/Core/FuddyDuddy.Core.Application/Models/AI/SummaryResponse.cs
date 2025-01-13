using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;
namespace FuddyDuddy.Core.Application.Models.AI;


public class SummaryResponse : IAiModelResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("article")]
    public string Article { get; set; } = string.Empty;
}