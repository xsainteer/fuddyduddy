using FuddyDuddy.Core.Application.Models;
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

    // New methods for digest caching
    Task AddDigestAsync(CachedDigestDto digest, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetLatestDigestsAsync<T>(
        int skip,
        int take,
        Language language = Language.RU,
        CancellationToken cancellationToken = default);
    Task<T?> GetDigestByIdAsync<T>(string id, CancellationToken cancellationToken = default);
    Task CacheDigestDtoAsync<T>(string id, T digest, CancellationToken cancellationToken = default);

    // Digest tweet timestamp
    Task<long?> GetLastTweetTimestampAsync(Language language, CancellationToken cancellationToken = default);
    Task SetLastTweetTimestampAsync(Language language, long timestamp, CancellationToken cancellationToken = default);

    // Twitter token
    Task<CachedTwitterTokenResponseDto?> GetTwitterTokenAsync(Language language, CancellationToken cancellationToken = default);
    Task SetTwitterTokenAsync(Language language, CachedTwitterTokenResponseDto token, CancellationToken cancellationToken = default);

    // Twitter auth state
    Task<CachedTwitterAuthStateDto?> GetTwitterAuthStateAsync(string state, CancellationToken cancellationToken = default);
    Task SetTwitterAuthStateAsync(string state, CachedTwitterAuthStateDto authState, CancellationToken cancellationToken = default);
    Task RemoveTwitterAuthStateAsync(string state, CancellationToken cancellationToken = default);
} 