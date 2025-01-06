using FuddyDuddy.Core.Domain.Dialects;

namespace FuddyDuddy.Core.Application.Services;

public class NewsSourceDialectFactory
{
    public INewsSourceDialect CreateDialect(string dialectType)
    {
        return dialectType switch
        {
            "KNews" => new KNewsDialect(),
            _ => throw new ArgumentException($"Unknown dialect type: {dialectType}")
        };
    }
}