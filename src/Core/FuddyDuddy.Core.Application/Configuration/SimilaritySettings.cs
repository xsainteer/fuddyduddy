namespace FuddyDuddy.Core.Application.Configuration;

public class SimilaritySettings
{
    public const string SectionName = "Similarity";
    public bool Enabled { get; set; } = true;
    public int MaxSimilarSummaries { get; set; } = 30;
}