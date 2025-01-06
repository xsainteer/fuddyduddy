using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Repositories;

public interface INewsArticleRepository
{
    Task<NewsArticle?> GetByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task AddAsync(NewsArticle article, CancellationToken cancellationToken = default);
    Task<IEnumerable<NewsArticle>> GetUnprocessedAsync(CancellationToken cancellationToken = default);
} 