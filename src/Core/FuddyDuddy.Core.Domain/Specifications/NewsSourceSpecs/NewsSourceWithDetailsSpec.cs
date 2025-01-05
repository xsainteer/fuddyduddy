using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Domain.Specifications.NewsSourceSpecs;

public class NewsSourceWithDetailsSpec : BaseSpecification<NewsSource>
{
    public NewsSourceWithDetailsSpec(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.RobotsTxt);
        // Add other includes as needed
    }
} 