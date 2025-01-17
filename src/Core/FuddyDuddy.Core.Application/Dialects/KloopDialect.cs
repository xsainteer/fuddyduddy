using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class KloopDialect : INewsSourceDialect
{
    public string SitemapUrl => "https://kloop.kg/post-sitemap40.xml";

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        // Parse XML content
        var doc = XDocument.Parse(content);
        var ns = doc.Root?.Name.Namespace;

        return doc.Descendants(ns + "url")
            .Select(url =>
            {
                var loc = url.Element(ns + "loc")?.Value;
                var lastmod = url.Element(ns + "lastmod")?.Value;

                if (string.IsNullOrEmpty(loc) || string.IsNullOrEmpty(lastmod))
                    return null;

                // Filter out non-article URLs (like images)
                if (!loc.Contains("/blog/"))
                    return null;

                // Extract title from URL path
                var urlParts = loc.Split('/');
                var title = urlParts.Length >= 2 
                    ? urlParts[^2].Replace('-', ' ') // Take second to last segment
                    : string.Empty;

                return new NewsItem(
                    Url: loc,
                    Title: title,
                    PublishedAt: DateTimeOffset.Parse(lastmod)
                );
            })
            .Where(item => item != null)
            .OrderByDescending(item => item.PublishedAt)
            .Take(20);
    }

    public string ExtractArticleContent(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Extract title
        var title = doc.DocumentNode
            .SelectSingleNode("//h1[@class='entry-title']")
            ?.InnerText.Trim() ?? string.Empty;

        // Extract content
        var content = doc.DocumentNode
            .SelectSingleNode("//div[contains(@class, 'td-post-content')]")
            ?.InnerText.Trim() ?? string.Empty;

        return $"{title}\n\n{content}";
    }
} 