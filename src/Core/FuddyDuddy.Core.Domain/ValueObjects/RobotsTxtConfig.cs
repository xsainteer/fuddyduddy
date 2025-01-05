namespace FuddyDuddy.Core.Domain.ValueObjects;

public record RobotsTxtConfig
{
    public string Url { get; }
    public int? CrawlDelay { get; }
    public IReadOnlyList<RobotsTxtRule> Rules { get; }
    public DateTimeOffset LastFetched { get; private set; }

    public RobotsTxtConfig(string url, int? crawlDelay, IEnumerable<RobotsTxtRule> rules)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        CrawlDelay = crawlDelay;
        Rules = rules?.ToList() ?? new List<RobotsTxtRule>();
        LastFetched = DateTimeOffset.UtcNow;
    }

    public void UpdateLastFetched()
    {
        LastFetched = DateTimeOffset.UtcNow;
    }
}

public record RobotsTxtRule
{
    public string UserAgent { get; }
    public IReadOnlyList<string> Allow { get; }
    public IReadOnlyList<string> Disallow { get; }

    public RobotsTxtRule(string userAgent, IEnumerable<string> allow, IEnumerable<string> disallow)
    {
        UserAgent = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
        Allow = allow?.ToList() ?? new List<string>();
        Disallow = disallow?.ToList() ?? new List<string>();
    }
} 