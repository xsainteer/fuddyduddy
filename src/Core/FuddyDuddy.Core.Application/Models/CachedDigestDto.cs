using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Models;

public class CachedDigestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public Language Language { get; set; }
    public List<CachedReferenceDto> References { get; set; } = new();

    public static CachedDigestDto FromDigest(Digest digest)
    {
        return new CachedDigestDto
        {
            Id = digest.Id,
            Title = digest.Title,
            Content = digest.Content,
            Language = digest.Language,
            GeneratedAt = digest.GeneratedAt,
            References = digest.References.Select(r => CachedReferenceDto.FromReference(r)).ToList()
        };
    }
}

public class CachedReferenceDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public static CachedReferenceDto FromReference(DigestReference reference)
    {
        return new CachedReferenceDto
        {
            Id = reference.Id,
            Title = reference.NewsSummary.Title,
            Url = reference.NewsSummary.NewsArticle.Url,
            Source = reference.NewsSummary.NewsArticle.NewsSource.Name,
            Reason = reference.Reason
        };
    }
}