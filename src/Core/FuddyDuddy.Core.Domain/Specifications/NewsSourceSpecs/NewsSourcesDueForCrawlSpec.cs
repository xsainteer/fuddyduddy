using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Specifications.NewsSourceSpecs;

public class NewsSourcesDueForCrawlSpec : BaseSpecification<NewsSource>
{
    public NewsSourcesDueForCrawlSpec(TimeSpan threshold) 
        : base(x => x.IsActive && 
                    DateTimeOffset.UtcNow - x.LastCrawled > threshold)
    {
        ApplyOrderBy(x => x.LastCrawled);
    }
} 