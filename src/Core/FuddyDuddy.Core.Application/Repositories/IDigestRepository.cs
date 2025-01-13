using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Repositories;

public interface IDigestRepository
{
    Task AddAsync(Digest digest, CancellationToken cancellationToken = default);
    Task<IEnumerable<Digest>> GetLatestAsync(int count, Language language, int skip = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<Digest>> GetLatestAsync(int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<Digest>> GetLatestAsync(Language language, DateTimeOffset lastTweetTime, CancellationToken cancellationToken = default);
    Task<Digest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Digest?> GetLatestByLanguageAsync(Language language, CancellationToken cancellationToken = default);
} 