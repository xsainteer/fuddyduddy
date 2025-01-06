using System.Xml.Linq;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Domain.Dialects;

public class KaktusDialect : INewsSourceDialect
{
    public string SitemapUrl
    {
        get
        {
            var now = DateTimeOffset.UtcNow;
            return $"https://kaktus.media/other/sitemap.php?sitemap=sitemap.xml&year={now.Year}&mount={now.Month:00}";
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

                if (string.IsNullOrEmpty(loc) || !loc.StartsWith("https://kaktus.media/doc/"))
                    return null;

                if (!DateTimeOffset.TryParse(lastmod, out var publishedAt))
                    return null;

                // Extract title from URL
                var titleSlug = loc.Split('/').Last().Split('_').Skip(1).ToList();
                if (titleSlug.Count == 0)
                    return null;

                titleSlug.RemoveAt(titleSlug.Count - 1); // Remove .html
                var title = string.Join(" ", titleSlug)
                    .Replace("\\", "")
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

        var articleContent = doc.DocumentNode.SelectSingleNode("//div[@class='Article--block-content']");
        return articleContent?.InnerText.Trim() ?? string.Empty;
    }
} 