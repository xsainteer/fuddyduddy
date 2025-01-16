namespace FuddyDuddy.Core.Domain.Entities;

public class Similar
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty; // common title for similar summaries
    public Language Language { get; private set; }

    //list of summaries
    public IReadOnlyCollection<SimilarReference> References => _references.AsReadOnly();
    private readonly List<SimilarReference> _references = new();

    private Similar() { } // For EF Core

    public Similar(string title, Language language, List<SimilarReference> references)
    {
        Id = Guid.NewGuid();
        Title = title.Length > 255 ? title[..255] : title;
        Language = language;
        _references.AddRange(references);

        // Set the back reference
        foreach (var reference in references)
        {
            reference.SetSimilar(this);
        }
    }

    public void AddReference(SimilarReference reference)
    {
        _references.Add(reference);
        reference.SetSimilar(this);
    }
}

public class SimilarReference
{
    public Guid Id { get; private set; }
    public Guid SimilarId { get; private set; }
    public Guid NewsSummaryId { get; private set; }
    public virtual NewsSummary NewsSummary { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    private SimilarReference() { } // For EF Core

    public SimilarReference(Guid newsSummaryId, string reason)
    {
        Id = Guid.NewGuid();
        NewsSummaryId = newsSummaryId;
        CreatedAt = DateTime.UtcNow;
        Reason = reason.Length > 255 ? reason[..255] : reason;
    }

    internal void SetSimilar(Similar similar)
    {
        SimilarId = similar.Id;
    }
}