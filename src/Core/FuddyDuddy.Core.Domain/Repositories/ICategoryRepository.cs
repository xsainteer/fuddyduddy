using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
} 