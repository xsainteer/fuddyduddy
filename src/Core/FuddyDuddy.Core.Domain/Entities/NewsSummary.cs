namespace FuddyDuddy.Core.Domain.Entities;

public enum NewsSummaryState
{
    Created,    // Initial state when summary is created
    Validated,  // Summary has been validated and is ready for digest
    Digested,   // Summary has been included in a digest
    Discarded   // Summary was found to be invalid/irrelevant
}

public enum Language
{
    RU = 0,     // Russian - original language
    EN = 1      // English - translation target
}

public static class LanguageExtensions
{
    public static string GetDescription(this Language language) => language switch
    {
        Language.RU => "Russian",
        Language.EN => "English",
        _ => throw new ArgumentOutOfRangeException(nameof(language))
    };
    public static string GetDescriptionInLocal(this Language language) => language switch
    {
        Language.RU => "Русский",
        Language.EN => "Английский",
        _ => throw new ArgumentOutOfRangeException(nameof(language))
    };
}

public class NewsSummary
{
    public Guid Id { get; private set; }
    public Guid NewsArticleId { get; private set; }
    public string Title { get; private set; }
    public string Article { get; private set; }
    public int CategoryId { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public NewsSummaryState State { get; private set; }
    public Language Language { get; private set; }
    public string? Reason { get; private set; }

    public virtual NewsArticle NewsArticle { get; private set; }
    public virtual Category Category { get; private set; }

    private NewsSummary() { } // For EF Core

    public NewsSummary(
        Guid newsArticleId,
        string title,
        string article,
        int categoryId,
        Language language = Language.RU)
    {
        Id = Guid.NewGuid();
        NewsArticleId = newsArticleId;
        Title = title;
        Article = article;
        CategoryId = categoryId;
        Language = language;
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

    public void UpdateCategory(int categoryId)
    {
        CategoryId = categoryId;
    }
} 