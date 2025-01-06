using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.Repositories;

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
} 