namespace FuddyDuddy.Core.Domain.Entities;

public class Similar
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty; // common title for similar summaries
    public string TitleLocal { get; private set; } = string.Empty; // common title for similar summaries in local language

    //list of summaries
    public IReadOnlyCollection<SimilarReference> Summaries => _summaries.AsReadOnly();
    private readonly List<SimilarReference> _summaries = new();

    private Similar() { } // For EF Core

    public Similar(string title, string titleLocal, List<SimilarReference> references)
    {
        Id = Guid.NewGuid();
        Title = title.Length > 255 ? title[..255] : title;
        TitleLocal = titleLocal.Length > 255 ? titleLocal[..255] : titleLocal;
        _summaries.AddRange(references);
    }
}

public class SimilarReference
{
    public Guid Id { get; private set; }
    public Guid SimilarId { get; private set; }
    public virtual Similar Similar { get; private set; }
    public Guid NewsSummaryId { get; private set; }
    public virtual NewsSummary NewsSummary { get; private set; }

    private SimilarReference() { } // For EF Core

    public SimilarReference(Guid similarId, Guid newsSummaryId, NewsSummary newsSummary)
    {
        Id = Guid.NewGuid();
        SimilarId = similarId;
        NewsSummaryId = newsSummaryId;
        NewsSummary = newsSummary;
    }
}