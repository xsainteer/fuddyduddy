using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface INewsSummaryRepository
{
    Task AddAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetByStateAsync(NewsSummaryState state, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetValidatedOrDigestedAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<NewsSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
} 