namespace FuddyDuddy.Core.Application.Dialects;

public interface INewsSourceDialect
{
    string SitemapUrl { get; }
    IEnumerable<NewsItem> ParseSitemap(string content);
    string ExtractArticleContent(string htmlContent);
}

public record NewsItem(
    string Url,
    string Title,
    DateTimeOffset PublishedAt); 