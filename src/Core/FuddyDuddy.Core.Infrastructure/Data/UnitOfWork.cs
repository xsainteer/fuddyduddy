using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FuddyDuddy.Core.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly FuddyDuddyDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public INewsSourceRepository NewsSourceRepository { get; }

    public UnitOfWork(FuddyDuddyDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        NewsSourceRepository = new NewsSourceRepository(context);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Transaction already in progress");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit");
            }

            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
            _currentTransaction?.Dispose();
        }
        _disposed = true;
    }
} 