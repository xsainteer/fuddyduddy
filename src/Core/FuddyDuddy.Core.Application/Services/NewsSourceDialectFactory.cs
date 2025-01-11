using FuddyDuddy.Core.Application.Dialects;

namespace FuddyDuddy.Core.Application.Services;

public interface INewsSourceDialectFactory
{
    INewsSourceDialect CreateDialect(string dialectType);
}

internal class NewsSourceDialectFactory : INewsSourceDialectFactory
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
            "24kg" => new TwentyFourKgDialect(),
            "Akchabar" => new AkchabarDialect(),
            _ => throw new ArgumentException($"Unknown dialect type: {dialectType}")
        };
    }
}