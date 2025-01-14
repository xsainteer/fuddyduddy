using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

internal class DigestRepository : IDigestRepository
{
    private readonly FuddyDuddyDbContext _context;

    public DigestRepository(FuddyDuddyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Digest digest, CancellationToken cancellationToken = default)
    {
        await _context.Digests.AddAsync(digest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Digest>> GetLatestAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .OrderByDescending(d => d.GeneratedAt)
            .Take(count)
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(ns => ns.Category)
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(ns => ns.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Digest>> GetLatestAsync(int count, Language language, int skip = 0, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .Where(d => d.Language == language)
            .OrderByDescending(d => d.GeneratedAt)
            .Skip(skip)
            .Take(count)
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(ns => ns.Category)
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(ns => ns.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Digest>> GetLatestAsync(Language language, DateTimeOffset lastTweetTime, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .Where(d => d.Language == language && d.GeneratedAt > lastTweetTime)
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .OrderByDescending(d => d.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Digest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(ns => ns.Category)
            .Include(d => d.References)
            .ThenInclude(r => r.NewsSummary)
            .ThenInclude(ns => ns.NewsArticle)
            .ThenInclude(na => na.NewsSource)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Digest?> GetLatestByLanguageAsync(Language language, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .Where(d => d.Language == language)
            .OrderByDescending(d => d.GeneratedAt)
            .Include(d => d.References)
            .FirstOrDefaultAsync(cancellationToken);
    }
} 