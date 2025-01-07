using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

public class NewsSummaryRepository : INewsSummaryRepository
{
    private readonly FuddyDuddyDbContext _context;

    public NewsSummaryRepository(FuddyDuddyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NewsSummary summary, CancellationToken cancellationToken = default)
    {
        await _context.NewsSummaries.AddAsync(summary, cancellationToken);
    }

    public async Task<IEnumerable<NewsSummary>> GetByStateAsync(NewsSummaryState state, CancellationToken cancellationToken = default)
    {
        return await _context
            .NewsSummaries
            .Where(s => s.State == state)
            .Include(s => s.Category)
            .Include(s => s.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(NewsSummary summary, CancellationToken cancellationToken = default)
    {
        _context.NewsSummaries.Update(summary);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NewsSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NewsSummaries
            .Include(s => s.NewsArticle)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
} 