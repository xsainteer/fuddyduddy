using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Repositories;

public interface INewsSummaryRepository
{
    Task AddAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get summaries by state, others are optional: date range, first, categoryId, language
    /// </summary>
    Task<IEnumerable<NewsSummary>> GetByStateAsync(
        IList<NewsSummaryState> states,
        DateTimeOffset? dateStart = null,
        DateTimeOffset? dateTo = null,
        int? first = null,
        int? categoryId = null,
        Language? language = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetValidatedOrDigestedAsync(int? first = null, Language? language = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<NewsSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetByNewsArticleIdAsync(Guid newsArticleId, CancellationToken cancellationToken = default);
    Task<NewsSummary> IncludeAllReferencesAsync(NewsSummary summary, CancellationToken cancellationToken = default);
} 