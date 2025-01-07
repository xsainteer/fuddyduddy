using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ICacheService
{
    Task AddSummaryAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> GetLatestSummariesAsync<T>(
        int skip,
        int take,
        Language language = Language.RU,
        int? categoryId = null,
        Guid? sourceId = null,
        CancellationToken cancellationToken = default);
    
    Task<T?> GetSummaryByIdAsync<T>(string id, CancellationToken cancellationToken = default);
    Task CacheSummaryDtoAsync<T>(string id, T summary, CancellationToken cancellationToken = default);
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
} 