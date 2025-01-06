using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ICacheService
{
    Task AddSummaryAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetLatestSummariesAsync<T>(int skip, int take, CancellationToken cancellationToken = default);
} 