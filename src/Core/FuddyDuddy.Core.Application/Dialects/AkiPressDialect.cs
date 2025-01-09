using System.Text.RegularExpressions;
using FuddyDuddy.Core.Domain.Entities;
using HtmlAgilityPack;

namespace FuddyDuddy.Core.Application.Dialects;

public class AkiPressDialect : INewsSourceDialect
{
    private static readonly Regex TimePattern = new(@"(\d{2}:\d{2})", RegexOptions.Compiled);
    
    public string SitemapUrl => "https://m.akipress.org/";

    public IEnumerable<NewsItem> ParseSitemap(string content)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);
        
        var newsItems = new List<NewsItem>();
        var newsContainer = document.DocumentNode.SelectSingleNode("//div[@id='last_news']");
        
        if (newsContainer == null)
            return newsItems;

        var newsRows = newsContainer.SelectNodes(".//div[contains(@class, 'simple-news-row')]");
        
        if (newsRows == null)
            return newsItems;

        foreach (var row in newsRows)
        {
            var timeSpan = row.SelectSingleNode(".//span")?.InnerText;
            var link = row.SelectSingleNode(".//a[@class='title']");
            
            if (string.IsNullOrEmpty(timeSpan) || link == null)
                continue;

            var timeMatch = TimePattern.Match(timeSpan);
            if (!timeMatch.Success)
                continue;

            var time = timeMatch.Groups[1].Value;
            var url = link.GetAttributeValue("href", "");
            var title = link.InnerText?.Trim();

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
                continue;

            // Convert relative URL to absolute
            if (url.StartsWith("//"))
                url = $"https:{url}";

            // Clean URL from query parameters
            var uri = new Uri(url);
            url = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";

            var publishedAt = DateTime.Today.Add(TimeSpan.Parse(time));
            
            newsItems.Add(new NewsItem(
                url,
                title,
                publishedAt
            ));
        }

        return newsItems;
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