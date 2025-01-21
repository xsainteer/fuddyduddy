namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class ChromaDbOptions
{
    public string Url { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
} 