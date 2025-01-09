namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class CrawlerOptions
{
    public const string SectionName = "Crawler";
    
    public TimeSpan? MinimumRequestInterval { get; set; }
    public string DefaultUserAgent { get; set; }
    public bool UseRandomDelay { get; set; } = true;
    public int MinDelayMilliseconds { get; set; } = 2000;
    public int MaxDelayMilliseconds { get; set; } = 5000;
    public bool UseProxies { get; set; } = false;
}