namespace FuddyDuddy.Core.Domain.Entities;

public class NewsSource
{
    public Guid Id { get; private set; }
    public string Domain { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset LastCrawled { get; private set; }
    public string DialectType { get; private set; }

    private NewsSource() { } // For EF Core

    public NewsSource(string domain, string name, string dialectType)
    {
        Id = Guid.NewGuid();
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DialectType = dialectType ?? throw new ArgumentNullException(nameof(dialectType));
        IsActive = true;
        LastCrawled = DateTimeOffset.UtcNow;
    }

    public void UpdateLastCrawled()
    {
        LastCrawled = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}