using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class AkiPressDialect : INewsSourceDialect
{
    private static readonly Regex TimePattern = new(@"(\d{2}:\d{2})", RegexOptions.Compiled);
    
    public string SitemapUrl => "https://akipress.org/";

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        // Find the main news table - more robust selector
        var newsTable = doc.DocumentNode.SelectSingleNode("//table[@id='last50']");
        if (newsTable == null)
            return Enumerable.Empty<NewsItem>();

        // Select rows that have time and news (more reliable structure-based selection)
        var newsRows = newsTable.SelectNodes(".//tr[td[@class='datetxt'] and .//a[@class='newslink']]");
        if (newsRows == null)
            return Enumerable.Empty<NewsItem>();

        var newsItems = new List<NewsItem>();
        var today = DateTimeOffset.UtcNow.Date;

        foreach (var row in newsRows)
        {
            var timeCell = row.SelectSingleNode(".//td[@class='datetxt']");
            var linkCell = row.SelectSingleNode(".//a[@class='newslink']");
            
            if (timeCell == null || linkCell == null)
                continue;

            // Skip rows that contain ads or other non-news content
            if (row.SelectNodes(".//ins") != null || row.SelectNodes(".//div[@class='inform_doska_center']") != null)
                continue;

            var timeMatch = TimePattern.Match(timeCell.InnerText);
            if (!timeMatch.Success)
                continue;

            var url = linkCell.GetAttributeValue("href", "");
            if (string.IsNullOrEmpty(url) || !url.Contains("/news:"))
                continue;

            // Make URL absolute if it's relative
            if (url.StartsWith("//"))
                url = "https:" + url;

            // Clean URL by removing query parameters
            // example: https://svodka.akipress.org/news:2213508?from=portal&place=lastmostread
            // we need to remove ?from=portal&place=lastmostread
            var questionMarkIndex = url.IndexOf('?');
            if (questionMarkIndex > 0)
            {
                url = url[..questionMarkIndex];
            }

            if (DateTimeOffset.TryParseExact(
                $"{today:yyyy-MM-dd} {timeMatch.Groups[1].Value}",
                "yyyy-MM-dd HH:mm",
                null,
                System.Globalization.DateTimeStyles.AssumeUniversal,
                out var publishedAt))
            {
                newsItems.Add(new NewsItem(url, linkCell.InnerText.Trim(), publishedAt));
            }
        }

        return newsItems.OrderByDescending(x => x.PublishedAt);
    }

    public string ExtractArticleContent(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var articleContent = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-detail')]")
            ?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'news_in')]")
            ?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-text')]");

        return articleContent?.InnerText.Trim() ?? string.Empty;
    }
}