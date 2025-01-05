using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FuddyDuddy.Core.Domain.ValueObjects;

public record RobotsTxtConfig
{
    public string Url { get; init; }
    public int? CrawlDelay { get; init; }
    public DateTimeOffset LastFetched { get; private set; }
    
    [NotMapped]
    private readonly List<RobotsTxtRule> _rules = new();
    
    [NotMapped]
    public IReadOnlyList<RobotsTxtRule> Rules => _rules.AsReadOnly();

    // This will be mapped by EF Core
    private string? RulesJson 
    {
        get => _rules.Any() ? JsonSerializer.Serialize(_rules) : null;
        set
        {
            _rules.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                var rules = JsonSerializer.Deserialize<List<RobotsTxtRule>>(value);
                if (rules != null)
                    _rules.AddRange(rules);
            }
        }
    }

    // Constructor for EF Core
    private RobotsTxtConfig() { }

    public RobotsTxtConfig(string url, int? crawlDelay)
    {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        CrawlDelay = crawlDelay;
        LastFetched = DateTimeOffset.UtcNow;
    }

    public void AddRule(RobotsTxtRule rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        _rules.Add(rule);
    }

    public void UpdateLastFetched()
    {
        LastFetched = DateTimeOffset.UtcNow;
    }
}

public record RobotsTxtRule
{
    public string UserAgent { get; init; }
    public IReadOnlyList<string> Allow { get; init; }
    public IReadOnlyList<string> Disallow { get; init; }

    // Constructor for JSON deserialization
    private RobotsTxtRule()
    {
        UserAgent = string.Empty;
        Allow = new List<string>();
        Disallow = new List<string>();
    }

    public RobotsTxtRule(string userAgent, IEnumerable<string> allow, IEnumerable<string> disallow)
    {
        UserAgent = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
        Allow = allow?.ToList() ?? new List<string>();
        Disallow = disallow?.ToList() ?? new List<string>();
    }
} 