using FuddyDuddy.Core.Domain.ValueObjects;
using FuddyDuddy.Core.Domain.Exceptions;

namespace FuddyDuddy.Core.Domain.Entities;

public class NewsSource
{
    public Guid Id { get; private set; }
    public string Domain { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset LastCrawled { get; private set; }
    
    private readonly List<SitemapConfig> _sitemaps = new();
    public IReadOnlyList<SitemapConfig> Sitemaps => _sitemaps.AsReadOnly();
    
    public RobotsTxtConfig? RobotsTxt { get; private set; }

    private NewsSource() { } // For EF Core

    public NewsSource(string domain, string name)
    {
        Id = Guid.NewGuid();
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsActive = true;
        LastCrawled = DateTimeOffset.UtcNow;
    }

    public void UpdateRobotsTxt(RobotsTxtConfig robotsTxt)
    {
        RobotsTxt = robotsTxt ?? throw new ArgumentNullException(nameof(robotsTxt));
    }

    public void AddSitemap(SitemapConfig sitemap)
    {
        if (sitemap == null) throw new ArgumentNullException(nameof(sitemap));
        if (_sitemaps.Any(s => s.Url == sitemap.Url))
            throw new DomainException($"Sitemap with URL {sitemap.Url} already exists");

        _sitemaps.Add(sitemap);
    }

    public void RemoveSitemap(string url)
    {
        var sitemap = _sitemaps.FirstOrDefault(s => s.Url == url);
        if (sitemap != null)
            _sitemaps.Remove(sitemap);
    }

    public void UpdateLastCrawled()
    {
        LastCrawled = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
} 