using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Domain.Specifications.NewsSourceSpecs;
using Microsoft.EntityFrameworkCore;

namespace FuddyDuddy.Core.Infrastructure.Data.Repositories;

public class NewsSourceRepository : BaseRepository<NewsSource, Guid>, INewsSourceRepository
{
    public NewsSourceRepository(FuddyDuddyDbContext context) : base(context)
    {
    }

    public async Task<NewsSource?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        var spec = new NewsSourceByDomainSpec(domain);
        return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NewsSource>> GetActiveSourcesAsync(CancellationToken cancellationToken = default)
    {
        var spec = new ActiveNewsSourcesSpec();
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NewsSource>> GetSourcesDueForCrawlAsync(TimeSpan threshold, CancellationToken cancellationToken = default)
    {
        var spec = new NewsSourcesDueForCrawlSpec(threshold);
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x => x.Domain == domain, cancellationToken);
    }
} 