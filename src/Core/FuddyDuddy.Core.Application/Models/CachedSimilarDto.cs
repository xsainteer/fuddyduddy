using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Models;

public class CachedSimilarDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public CachedSimilarReferenceDto[] References { get; set; } = [];

    public static CachedSimilarDto FromSimilar(Similar similar)
    {
        return new CachedSimilarDto
        {
            Id = similar.Id,
            Title = similar.Title,
            References = similar.References.Select(CachedSimilarReferenceDto.FromSimilarReference).ToArray()
        };
    }
}


public class CachedSimilarReferenceBaseDto
{
    public Guid Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public string Title { get; set; } = string.Empty;

    public static CachedSimilarReferenceBaseDto FromSimilarReference(SimilarReference reference)
    {
        return new CachedSimilarReferenceBaseDto
        {
            Id = reference.Id,
            Source = reference.NewsSummary.NewsArticle.NewsSource.Name,
            GeneratedAt = reference.NewsSummary.GeneratedAt,
            Title = reference.NewsSummary.Title
        };
    }
}

public class CachedSimilarReferenceDto : CachedSimilarReferenceBaseDto
{
    public string Article { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// References should be included (inner joined)
    /// </summary>
    public static CachedSimilarReferenceDto FromSimilarReference(SimilarReference reference)
    {
        var baseDto = CachedSimilarReferenceBaseDto.FromSimilarReference(reference);
        return new CachedSimilarReferenceDto
        {
            Id = baseDto.Id,
            Source = baseDto.Source,
            GeneratedAt = baseDto.GeneratedAt,
            Title = baseDto.Title,
            Article = reference.NewsSummary.Article,
            Reason = reference.Reason
        };
    }
}