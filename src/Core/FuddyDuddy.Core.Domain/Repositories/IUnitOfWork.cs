namespace FuddyDuddy.Core.Domain.Repositories;

public interface IUnitOfWork
{
    INewsSourceRepository NewsSourceRepository { get; }
    INewsArticleRepository NewsArticleRepository { get; }
    INewsSummaryRepository NewsSummaryRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 