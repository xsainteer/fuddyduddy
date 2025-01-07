using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

public class DigestRepository : IDigestRepository
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

    public async Task<IEnumerable<Digest>> GetLatestAsync(int count, Language language, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .Where(d => d.Language == language)
            .OrderByDescending(d => d.GeneratedAt)
            .Take(count)
            .Include(d => d.References)
            .ToListAsync(cancellationToken);
    }

    public async Task<Digest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Digests
            .Include(d => d.References)
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