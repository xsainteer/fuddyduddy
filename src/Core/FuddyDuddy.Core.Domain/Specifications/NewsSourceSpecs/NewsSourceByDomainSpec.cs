using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Specifications.NewsSourceSpecs;

public class NewsSourceByDomainSpec : BaseSpecification<NewsSource>
{
    public NewsSourceByDomainSpec(string domain) 
        : base(x => x.Domain == domain)
    {
        AddInclude(x => x.RobotsTxt);
    }
} 