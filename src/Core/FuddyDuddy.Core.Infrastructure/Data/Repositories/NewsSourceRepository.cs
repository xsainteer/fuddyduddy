using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

internal class NewsSourceRepository : INewsSourceRepository
{
    private readonly FuddyDuddyDbContext _context;

    public NewsSourceRepository(FuddyDuddyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NewsSource>> GetActiveSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.NewsSources
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<NewsSource?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _context.NewsSources
            .FirstOrDefaultAsync(x => x.Domain == domain, cancellationToken);
    }

    public async Task UpdateAsync(NewsSource source, CancellationToken cancellationToken = default)
    {
        _context.NewsSources.Update(source);
        await _context.SaveChangesAsync(cancellationToken);
    }
} 