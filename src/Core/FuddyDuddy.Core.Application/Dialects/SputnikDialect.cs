using System.Xml.Linq;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class SputnikDialect : INewsSourceDialect
{
    public string SitemapUrl
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            var twoDaysAgo = now.AddDays(-2);
            
            return $"https://ru.sputnik.kg/sitemap_article.xml" +
                   $"?date_start={twoDaysAgo:yyyyMMdd}" +
                   $"&date_end={now:yyyyMMdd}";
        }
    }

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        var doc = XDocument.Parse(content);
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");

        if (doc.Root == null)
            return Enumerable.Empty<NewsItem>();

        return doc.Root
            .Elements(ns + "url")
            .Select(url =>
            {
                var loc = url.Element(ns + "loc")?.Value ?? string.Empty;
                var lastmod = url.Element(ns + "lastmod")?.Value ?? string.Empty;

                if (string.IsNullOrEmpty(loc) || !loc.StartsWith("https://ru.sputnik.kg/"))
                    return null;

                if (!DateTimeOffset.TryParse(lastmod, out var publishedAt))
                    return null;

                // Extract title from URL
                var titleSlug = loc.Split('/').Last().Split('-').ToList();
                if (titleSlug.Count == 0)
                    return null;

                if (titleSlug.Last().EndsWith(".html"))
                {
                    titleSlug[titleSlug.Count - 1] = titleSlug.Last()[..^5]; // Remove .html
                }

                var title = string.Join(" ", titleSlug)
                    .Replace("_", " ")
                    .Trim();

                return new NewsItem(loc, title, publishedAt);
            })
            .Where(item => item != null)!
            .OrderByDescending(x => x.PublishedAt)
            .ToList();
    }

    public string ExtractArticleContent(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var articleContent = doc.DocumentNode.SelectSingleNode("//div[@class='article ']");
        return articleContent?.InnerText.Trim() ?? string.Empty;
    }
} 