using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface INewsSourceRepository
{
    Task<IEnumerable<NewsSource>> GetActiveSourcesAsync(CancellationToken cancellationToken = default);
    Task<NewsSource?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task UpdateAsync(NewsSource source, CancellationToken cancellationToken = default);
}