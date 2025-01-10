using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Models.AI;


public class SummaryResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("article")]
    public string Article { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public int CategoryId { get; set; } = 0;
}