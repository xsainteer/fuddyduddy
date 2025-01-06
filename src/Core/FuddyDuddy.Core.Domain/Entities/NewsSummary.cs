namespace FuddyDuddy.Core.Domain.Entities;

public enum NewsSummaryState
{
    Created,    // Initial state when summary is created
    Validated,  // Summary has been validated and is ready for digest
    Digested,   // Summary has been included in a digest
    Discarded   // Summary was found to be invalid/irrelevant
}

public class NewsSummary
{
    public Guid Id { get; private set; }
    public Guid NewsArticleId { get; private set; }
    public string Title { get; private set; }
    public string Article { get; private set; }
    public List<string> Tags { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public NewsSummaryState State { get; private set; }
    public string? Reason { get; private set; }

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
        State = NewsSummaryState.Created;
    }

    public void Validate(string? reason = null)
    {
        State = NewsSummaryState.Validated;
        Reason = reason;
    }

    public void Discard(string reason)
    {
        State = NewsSummaryState.Discarded;
        Reason = reason;
    }

    public void MarkAsDigested()
    {
        State = NewsSummaryState.Digested;
    }
} 