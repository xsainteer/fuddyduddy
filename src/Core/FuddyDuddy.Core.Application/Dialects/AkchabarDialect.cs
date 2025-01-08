using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class AkchabarDialect : INewsSourceDialect
{
    public string SitemapUrl => "https://www.akchabar.kg/sitemap.xml";

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        var doc = XDocument.Parse(content);
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var newsNs = XNamespace.Get("http://www.google.com/schemas/sitemap-news/0.9");

        var urlElements = doc.Descendants(ns + "url");
        var newsItems = new List<NewsItem>();

        foreach (var urlElement in urlElements)
        {
            var loc = urlElement.Element(ns + "loc")?.Value;
            if (string.IsNullOrEmpty(loc) || !loc.Contains("/news/"))
                continue;

            var newsElement = urlElement.Element(newsNs + "news");
            if (newsElement == null)
                continue;

            var title = newsElement.Element(newsNs + "title")?.Value;
            if (string.IsNullOrEmpty(title))
                continue;

            var dateStr = newsElement.Element(newsNs + "publication_date")?.Value;
            if (string.IsNullOrEmpty(dateStr) || !DateTimeOffset.TryParse(dateStr, out var publishedAt))
                continue;

            newsItems.Add(new NewsItem(loc, title, publishedAt));
        }

        return newsItems.OrderByDescending(x => x.PublishedAt);
    }

    public string ExtractArticleContent(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Find the first article container div
        var articleContainer = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'w-full py-4 lg:py-6 flex')]");
        if (articleContainer == null)
            return string.Empty;

        var contentBuilder = new System.Text.StringBuilder();

        // Get title
        var titleElement = articleContainer.SelectSingleNode(".//h1[contains(@class, 'font-semibold')]");
        if (titleElement != null)
        {
            contentBuilder.AppendLine(titleElement.InnerText.Trim());
            contentBuilder.AppendLine();
        }

        // Get main content from CKEditor div
        var mainContent = articleContainer.SelectSingleNode(".//div[contains(@class, 'ckeditor')]");
        if (mainContent == null)
            return string.Empty;

        // Clean up the content by removing unnecessary elements
        var paragraphs = mainContent.SelectNodes(".//p|.//table");
        if (paragraphs == null)
            return string.Empty;

        foreach (var paragraph in paragraphs)
        {
            var text = paragraph.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                contentBuilder.AppendLine(text);
                contentBuilder.AppendLine();
            }
        }

        return contentBuilder.ToString().Trim();
    }
} 