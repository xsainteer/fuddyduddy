using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    private readonly FuddyDuddyDbContext _context;

    public NewsArticleRepository(FuddyDuddyDbContext context)
    {
        _context = context;
    }

    public async Task<NewsArticle?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _context.NewsArticles
            .FirstOrDefaultAsync(x => x.Url == url, cancellationToken);
    }

    public async Task AddAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        await _context.NewsArticles.AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<NewsArticle>> GetUnprocessedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.NewsArticles
            .Where(x => !x.IsProcessed)
            .OrderBy(x => x.PublishedAt)
            .ToListAsync(cancellationToken);
    }
}