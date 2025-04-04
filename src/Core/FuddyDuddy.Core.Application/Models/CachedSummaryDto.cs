using FuddyDuddy.Core.Domain.Entities;
using System.Linq;

namespace FuddyDuddy.Core.Application.Models;

public class CachedSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryLocal { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public string? Reason { get; set; }
    public string NewsArticleTitle { get; set; } = string.Empty;
    public string NewsArticleUrl { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<CachedSimilarReferenceBaseDto> Similarities { get; set; } = [];
    /// <summary>
    /// Add similarities to the summary
    /// NOTE: References should be included
    /// </summary>
    /// <param name="similar"></param>
    public void AddBaseSimilarities(Similar similar)
    {
        Similarities ??= [];
        Similarities.AddRange(similar
            .References
            .Where(r => r.NewsSummaryId != Id)
            .Select(CachedSimilarReferenceBaseDto.FromSimilarReference)
            .ToArray());

        Similarities = Similarities
            .OrderByDescending(s => s.GeneratedAt)
            .Take(3)
            .ToList();
    }

    public static CachedSummaryDto FromNewsSummary(NewsSummary summary)
    {
        return new CachedSummaryDto
        {
            Id = summary.Id,
            Title = summary.Title ?? string.Empty,
            Article = summary.Article ?? string.Empty,
            Category = summary.Category?.Name ?? string.Empty,
            CategoryLocal = summary.Category?.Local ?? string.Empty,
            GeneratedAt = summary.GeneratedAt,
            Reason = summary.Reason ?? string.Empty,
            NewsArticleTitle = summary.NewsArticle?.Title ?? string.Empty, 
            NewsArticleUrl = summary.NewsArticle?.Url ?? string.Empty,
            Source = summary.NewsArticle?.NewsSource?.Name ?? string.Empty 
        };
    }
} 