using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

internal class SimilarRepository : ISimilarRepository
{
    private readonly FuddyDuddyDbContext _context;

    public SimilarRepository(FuddyDuddyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Similar similar, CancellationToken cancellationToken = default)
    {
        await _context.Similars.AddAsync(similar, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddReferenceAsync(Similar similar, SimilarReference reference, CancellationToken cancellationToken = default)
    {
        similar.AddReference(reference);
        _context.Update(similar);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Similar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Similars
            .Include(s => s.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(s => s.Category)
            .Include(s => s.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(s => s.NewsArticle)
            .ThenInclude(a => a.NewsSource)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Similar>> GetBySummaryIdAsync(Guid summaryId, CancellationToken cancellationToken = default)
    {
        return await _context.Similars
            .Include(s => s.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(s => s.Category)
            .Include(s => s.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(s => s.NewsArticle)
            .ThenInclude(a => a.NewsSource)
            .Where(s => s.References.Any(r => r.NewsSummaryId == summaryId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Similar>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Similars
            .Include(s => s.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(s => s.Category)
            .Include(s => s.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(s => s.NewsArticle)
            .ThenInclude(a => a.NewsSource)
            .OrderByDescending(s => s.References.Max(r => r.NewsSummary.GeneratedAt))
            .Take(count)
            .ToListAsync(cancellationToken);
    }
} 