namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class ChromaDbOptions
{
    public string Url { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public int RatePerMinute { get; set; } = 0;
    public List<Metadata>? CollectionMetadata { get; set; } = null;

    public class Metadata
    {
        public string Key { get; set; } = string.Empty;
        public object Value { get; set; } = string.Empty;
    }
}
