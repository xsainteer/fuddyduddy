using FuddyDuddy.Core.Application.Dialects;

namespace FuddyDuddy.Core.Application.Services;

public class NewsSourceDialectFactory
{
    public NewsSourceDialectFactory()
    {
    }

    public INewsSourceDialect CreateDialect(string dialectType)
    {
        return dialectType switch
        {
            "KNews" => new KNewsDialect(),
            "Kaktus" => new KaktusDialect(),
            "Sputnik" => new SputnikDialect(),
            "AkiPress" => new AkiPressDialect(),
            _ => throw new ArgumentException($"Unknown dialect type: {dialectType}")
        };
    }
}