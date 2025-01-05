using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Specifications.NewsSourceSpecs;

public class ActiveNewsSourcesSpec : BaseSpecification<NewsSource>
{
    public ActiveNewsSourcesSpec() : base(x => x.IsActive)
    {
        ApplyOrderBy(x => x.LastCrawled);
    }
} 