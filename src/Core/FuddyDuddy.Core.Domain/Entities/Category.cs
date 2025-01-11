namespace FuddyDuddy.Core.Domain.Entities;

public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Local { get; private set; }
    public string Keywords { get; private set; }
    public string KeywordsLocal { get; private set; }

    private Category() { } // For EF Core

    public Category(int id, string name, string local, string keywords, string keywordsLocal)
    {
        Id = id;
        Name = name;
        Local = local;
        Keywords = keywords;
        KeywordsLocal = keywordsLocal;
    }
} 