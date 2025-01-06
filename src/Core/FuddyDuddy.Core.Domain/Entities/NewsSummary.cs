namespace FuddyDuddy.Core.Domain.Entities;

public class NewsSummary
{
    public Guid Id { get; private set; }
    public Guid NewsArticleId { get; private set; }
    public string Title { get; private set; }
    public string Article { get; private set; }
    public List<string> Tags { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }

    public virtual NewsArticle NewsArticle { get; private set; }

    private NewsSummary() { } // For EF Core

    public NewsSummary(
        Guid newsArticleId,
        string title,
        string article,
        IEnumerable<string> tags)
    {
        Id = Guid.NewGuid();
        NewsArticleId = newsArticleId;
        Title = title;
        Article = article;
        Tags = tags.ToList();
        GeneratedAt = DateTimeOffset.UtcNow;
    }
} 