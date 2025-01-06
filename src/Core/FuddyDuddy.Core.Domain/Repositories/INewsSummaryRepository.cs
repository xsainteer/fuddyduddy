using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface INewsSummaryRepository
{
    Task AddAsync(NewsSummary summary, CancellationToken cancellationToken = default);
} 