using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface IDigestRepository
{
    Task AddAsync(Digest digest, CancellationToken cancellationToken = default);
    Task<IEnumerable<Digest>> GetLatestAsync(int count, Language language, CancellationToken cancellationToken = default);
    Task<Digest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Digest?> GetLatestByLanguageAsync(Language language, CancellationToken cancellationToken = default);
} 