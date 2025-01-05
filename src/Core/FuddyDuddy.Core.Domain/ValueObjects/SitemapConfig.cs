namespace FuddyDuddy.Core.Domain.ValueObjects;

public record SitemapConfig
{
    public string Url { get; }
    public SitemapType Type { get; }
    public TimeSpan UpdateFrequency { get; }
    public DateTimeOffset LastSuccessfulFetch { get; private set; }

    public SitemapConfig(string url, SitemapType type, TimeSpan updateFrequency)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Type = type;
        UpdateFrequency = updateFrequency;
        LastSuccessfulFetch = DateTimeOffset.UtcNow;
    }

    public void UpdateLastFetch()
    {
        LastSuccessfulFetch = DateTimeOffset.UtcNow;
    }

    public bool ShouldFetch() =>
        DateTimeOffset.UtcNow - LastSuccessfulFetch > UpdateFrequency;
}

public enum SitemapType
{
    Standard,
    News,
    Index
} 