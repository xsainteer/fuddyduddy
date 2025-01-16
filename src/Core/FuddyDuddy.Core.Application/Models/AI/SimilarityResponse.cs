using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Application.Models.AI;

public class SimilarityResponse : IAiModelResponse
{
    [JsonPropertyName("similar_summary_id")]
    public Guid SimilarSummaryId { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}