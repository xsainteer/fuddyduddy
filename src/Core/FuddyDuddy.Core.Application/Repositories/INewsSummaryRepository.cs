using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Repositories;

public interface INewsSummaryRepository
{
    Task AddAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetByStateAsync(IList<NewsSummaryState> states, DateTimeOffset? date = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetValidatedOrDigestedAsync(int? first = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(NewsSummary summary, CancellationToken cancellationToken = default);
    Task<NewsSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsSummary>> GetByNewsArticleIdAsync(Guid newsArticleId, CancellationToken cancellationToken = default);
} 