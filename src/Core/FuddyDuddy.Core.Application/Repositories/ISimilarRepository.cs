using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Repositories;

public interface ISimilarRepository
{
    Task AddAsync(Similar similar, CancellationToken cancellationToken = default);
    Task AddReferenceAsync(Similar similar, SimilarReference reference, CancellationToken cancellationToken = default);
    Task<Similar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Similar>> GetBySummaryIdAsync(Guid summaryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Similar>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<IDictionary<Guid, IEnumerable<NewsSummary>>> GetGroupedSummariesWithConnectedOnesAsync(int numberOfLatestSimilars, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a similar and returns ids of news summaries that are no longer connected to any similar
    /// </summary>
    Task<IEnumerable<Guid>> DeleteSimilarAsync(Guid similarId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a similar reference and returns ids of news summaries that are no longer connected to removed similar reference
    /// </summary>
    Task<IEnumerable<Guid>> DeleteSimilarReferenceAsync(Guid newsSummaryId, CancellationToken cancellationToken = default);
} 
