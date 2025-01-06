using System.Xml.Linq;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class KNewsDialect : INewsSourceDialect
{
    public string SitemapUrl => "https://knews.kg/sitemap-news.xml";

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        var doc = XDocument.Parse(content);
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var newsNs = XNamespace.Get("http://www.google.com/schemas/sitemap-news/0.9");

        return doc.Root?
            .Elements(ns + "url")
            .Select(url => new NewsItem(
                url.Element(ns + "loc")?.Value ?? string.Empty,
                url.Element(newsNs + "news")?
                    .Element(newsNs + "title")?.Value ?? string.Empty,
                DateTimeOffset.Parse(
                    url.Element(newsNs + "news")?
                        .Element(newsNs + "publication_date")?.Value ?? 
                    DateTimeOffset.MinValue.ToString())
            ))
            .Where(item => !string.IsNullOrEmpty(item.Url))
            .OrderByDescending(x => x.PublishedAt)
            .ToList() ?? Enumerable.Empty<NewsItem>();
    }

    public string ExtractArticleContent(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var articleContent = doc.DocumentNode.SelectSingleNode("//div[@class='td-post-content']");
        return articleContent?.InnerText.Trim() ?? string.Empty;
    }
} 