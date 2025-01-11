namespace FuddyDuddy.Core.Application.Models.Broker;

public record SummaryRequest(
    Guid NewsArticleId,
    string Title,
    string Content,
    string Language,
    string? SourceName = null); 