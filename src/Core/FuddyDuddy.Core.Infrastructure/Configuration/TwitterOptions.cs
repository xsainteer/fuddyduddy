using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class TwitterOptions
{
    public bool Enabled { get; set; }
    public Dictionary<Language, Details> LanguageDict { get; set; } = new();

    public class Details
    {
        // for OAuth 2.0
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
    }
}