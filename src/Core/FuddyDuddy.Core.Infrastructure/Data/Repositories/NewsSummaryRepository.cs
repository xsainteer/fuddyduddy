using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

internal class NewsSummaryRepository : INewsSummaryRepository
{
    private readonly FuddyDuddyDbContext _context;

    public NewsSummaryRepository(FuddyDuddyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NewsSummary summary, CancellationToken cancellationToken = default)
    {
        await _context.NewsSummaries.AddAsync(summary, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<NewsSummary>> GetByStateAsync(
        IList<NewsSummaryState> states,
        DateTimeOffset? dateStart = null,
        DateTimeOffset? dateTo = null,
        int? first = null,
        int? categoryId = null,
        Language? language = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context
            .NewsSummaries
            .Where(s => states.Contains(s.State))
            .Where(s => dateStart == null || s.GeneratedAt >= dateStart)
            .Where(s => dateTo == null || s.GeneratedAt <= dateTo)
            .Where(s => categoryId == null || s.CategoryId == categoryId)
            .Where(s => language == null || s.Language == language)
            .Include(s => s.Category)
            .Include(s => s.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .OrderByDescending(s => s.GeneratedAt)
            .AsQueryable();

        if (first != null)
        {
            query = query.Take(first.Value);
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NewsSummary>> GetValidatedOrDigestedAsync(int? first = null, Language? language = null, CancellationToken cancellationToken = default)
    {
        var query = _context
            .NewsSummaries
            .Where(s => s.State == NewsSummaryState.Validated || s.State == NewsSummaryState.Digested)
            .Where(s => language == null || s.Language == language)
            .Include(s => s.Category)
            .Include(s => s.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .OrderByDescending(s => s.GeneratedAt)
            .AsQueryable();
        if (first != null)
        {
            query = query.Take(first.Value);
        }
        return await query.ToListAsync(cancellationToken);
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
            .Include(s => s.Category)
            .Include(s => s.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<NewsSummary>> GetByNewsArticleIdAsync(Guid newsArticleId, CancellationToken cancellationToken = default)
    {
        return await _context.NewsSummaries
            .Where(s => s.NewsArticleId == newsArticleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<NewsSummary> IncludeAllReferencesAsync(NewsSummary summary, CancellationToken cancellationToken = default)
    {
        // Load Category
        await _context.Entry(summary)
            .Reference(s => s.Category)
            .LoadAsync(cancellationToken);

        // Load NewsArticle
        await _context.Entry(summary)
            .Reference(s => s.NewsArticle)
            .LoadAsync(cancellationToken);

        // Load NewsSource after NewsArticle is loaded
        if (summary.NewsArticle != null)
        {
            await _context.Entry(summary.NewsArticle)
                .Reference(na => na.NewsSource)
                .LoadAsync(cancellationToken);
        }

        return summary;
    }
} 