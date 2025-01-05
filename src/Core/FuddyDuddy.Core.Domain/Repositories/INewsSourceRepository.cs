using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface INewsSourceRepository
{
    Task<NewsSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NewsSource?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsSource>> GetActiveSourcesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsSource>> GetSourcesDueForCrawlAsync(TimeSpan threshold, CancellationToken cancellationToken = default);
    Task AddAsync(NewsSource newsSource, CancellationToken cancellationToken = default);
    Task UpdateAsync(NewsSource newsSource, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string domain, CancellationToken cancellationToken = default);
}