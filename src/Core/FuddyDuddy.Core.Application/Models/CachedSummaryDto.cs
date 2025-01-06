using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Models;

public class CachedSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
    public string? Reason { get; set; }
    public string NewsArticleTitle { get; set; } = string.Empty;
    public string NewsArticleUrl { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public static CachedSummaryDto FromNewsSummary(NewsSummary summary)
    {
        return new CachedSummaryDto
        {
            Id = summary.Id,
            Title = summary.Title,
            Article = summary.Article,
            Tags = summary.Tags,
            GeneratedAt = summary.GeneratedAt,
            Reason = summary.Reason,
            NewsArticleTitle = summary.NewsArticle.Title,
            NewsArticleUrl = summary.NewsArticle.Url,
            Source = summary.NewsArticle.NewsSource.Name
        };
    }
} 