namespace FuddyDuddy.Core.Domain.Entities;

public class NewsArticle
{
    public Guid Id { get; private set; }
    public Guid NewsSourceId { get; private set; }
    public string Url { get; private set; }
    public string Title { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; }
    public DateTimeOffset CollectedAt { get; private set; }
    public bool IsProcessed { get; private set; }
    
    public virtual NewsSource NewsSource { get; private set; }

    private NewsArticle() { } // For EF Core

    public NewsArticle(
        Guid newsSourceId,
        string url,
        string title,
        DateTimeOffset publishedAt)
    {
        Id = Guid.NewGuid();
        NewsSourceId = newsSourceId;
        Url = url;
        Title = title;
        PublishedAt = publishedAt;
        CollectedAt = DateTimeOffset.UtcNow;
        IsProcessed = false;
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }
} 