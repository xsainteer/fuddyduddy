namespace FuddyDuddy.Core.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    
    INewsSourceRepository NewsSourceRepository { get; }
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 