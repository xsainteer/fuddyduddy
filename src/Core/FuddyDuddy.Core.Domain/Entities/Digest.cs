namespace FuddyDuddy.Core.Domain.Entities;


public enum DigestState
{
    Created,    // Initial state when digest is created
    Published,   // Digest has been published
    Discarded   // Digest was found to be invalid/irrelevant
}

public class Digest
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public Language Language { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public IReadOnlyCollection<DigestReference> References => _references.AsReadOnly();
    public DigestState State { get; private set; }

    private readonly List<DigestReference> _references = new();

    private Digest() { } // For EF Core

    public Digest(
        string title,
        string content,
        Language language,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        List<DigestReference> references,
        DigestState state)
    {
        Id = Guid.NewGuid();
        Title = title;
        Content = content;
        Language = language;
        GeneratedAt = DateTimeOffset.UtcNow;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        State = state;
        _references.AddRange(references);

        // Set the back reference
        foreach (var reference in references)
        {
            reference.SetDigest(this);
        }
    }
}

public class DigestReference
{
    public Guid Id { get; private set; }
    public Guid DigestId { get; private set; }
    public Guid NewsSummaryId { get; private set; }
    public virtual NewsSummary NewsSummary { get; private set; }
    public string Title { get; private set; }
    public string Url { get; private set; }
    public string Reason { get; private set; }

    private DigestReference() { } // For EF Core

    public DigestReference(
        Guid newsSummaryId,
        string title,
        string url,
        string reason)
    {
        Id = Guid.NewGuid();
        NewsSummaryId = newsSummaryId;
        Title = title;
        Url = url;
        Reason = reason;
    }

    internal void SetDigest(Digest digest)
    {
        DigestId = digest.Id;
    }
} 