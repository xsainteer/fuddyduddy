using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class TwentyFourKgDialect : INewsSourceDialect
{
    private static readonly Regex TimePattern = new(@"(\d{2}:\d{2})", RegexOptions.Compiled);
    
    public string SitemapUrl => "https://24.kg/";

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        // Find the main news container
        var newsContainer = doc.DocumentNode.SelectSingleNode("//div[@id='newslist']");
        if (newsContainer == null)
            return Enumerable.Empty<NewsItem>();

        // Select all news items
        var newsItems = newsContainer.SelectNodes(".//div[@class='one']");
        if (newsItems == null)
            return Enumerable.Empty<NewsItem>();

        var result = new List<NewsItem>();
        var today = DateTimeOffset.UtcNow.Date;

        foreach (var item in newsItems)
        {
            var timeElement = item.SelectSingleNode(".//div[@class='time']");
            var titleElement = item.SelectSingleNode(".//div[@class='title']/a");
            
            if (timeElement == null || titleElement == null)
                continue;

            var timeMatch = TimePattern.Match(timeElement.InnerText);
            if (!timeMatch.Success)
                continue;

            var url = titleElement.GetAttributeValue("href", "");
            if (string.IsNullOrEmpty(url))
                continue;

            // Make URL absolute if it's relative
            if (url.StartsWith("/"))
                url = "https://24.kg" + url;

            if (DateTimeOffset.TryParseExact(
                $"{today:yyyy-MM-dd} {timeMatch.Groups[1].Value}",
                "yyyy-MM-dd HH:mm",
                null,
                System.Globalization.DateTimeStyles.AssumeUniversal,
                out var publishedAt))
            {
                result.Add(new NewsItem(url, titleElement.InnerText.Trim(), publishedAt));
            }
        }

        return result.OrderByDescending(x => x.PublishedAt);
    }

    public string ExtractArticleContent(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var contentBuilder = new System.Text.StringBuilder();

        // Extract title
        var titleElement = doc.DocumentNode.SelectSingleNode("//h1[@class='newsTitle']");
        if (titleElement != null)
        {
            contentBuilder.AppendLine(titleElement.InnerText.Trim());
            contentBuilder.AppendLine();
        }

        // Extract content
        var articleContent = doc.DocumentNode.SelectSingleNode("//div[@class='cont']");
        if (articleContent != null)
        {
            contentBuilder.AppendLine(articleContent.InnerText.Trim());
        }

        return contentBuilder.ToString().Trim();
    }
} 