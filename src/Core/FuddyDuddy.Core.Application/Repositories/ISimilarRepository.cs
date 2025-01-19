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
} 