using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ICacheService
{
    Task AddSummaryAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetLatestSummariesAsync<T>(int skip, int take, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>?> GetSummariesAroundIdAsync<T>(string summaryId, int count, CancellationToken cancellationToken = default);
    Task<T?> GetSummaryByIdAsync<T>(string id, CancellationToken cancellationToken = default);
    Task CacheSummaryDtoAsync<T>(string id, T summary, CancellationToken cancellationToken = default);
    Task RebuildCacheAsync(IEnumerable<NewsSummary> summaries, CancellationToken cancellationToken = default);
} 