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

    public async Task<IDictionary<Guid, IEnumerable<NewsSummary>>> GetGroupedSummariesWithConnectedOnesAsync(int numberOfLatestSimilars, CancellationToken cancellationToken = default)
    {
        var similars = (await GetRecentAsync(numberOfLatestSimilars, cancellationToken)).ToDictionary(s => s.Id, s => s);
        return similars.SelectMany(s => s.Value.References).ToDictionary(s => s.NewsSummaryId, s => similars[s.SimilarId].References.Select(r => r.NewsSummary));
    }

    public async Task<IEnumerable<Guid>> DeleteSimilarAsync(Guid similarId, CancellationToken cancellationToken = default)
    {
        var similar = await _context
            .Similars
            .Include(s => s.References)
            .FirstOrDefaultAsync(s => s.Id == similarId, cancellationToken);

        if (similar != null)
        {
            var newsSummaryIds = similar.References.Select(r => r.NewsSummaryId).ToList();
            _context.Similars.Remove(similar);
            await _context.SaveChangesAsync(cancellationToken);
            return newsSummaryIds;
        }
        return [];
    }

    public async Task DeleteSimilarReferenceAsync(Guid newsSummaryId, CancellationToken cancellationToken = default)
    {
        var similarReferences = await _context.SimilarReferences.Where(s => s.NewsSummaryId == newsSummaryId).ToListAsync(cancellationToken);
        if (similarReferences.Any())
        {
            _context.SimilarReferences.RemoveRange(similarReferences);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 