namespace FuddyDuddy.Core.Domain.Entities;

public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Local { get; private set; }

    private Category() { } // For EF Core

    public Category(int id, string name, string local)
    {
        Id = id;
        Name = name;
        Local = local;
    }
} 