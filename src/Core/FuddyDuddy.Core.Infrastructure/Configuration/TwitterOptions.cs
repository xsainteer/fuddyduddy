using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class TwitterOptions
{
    public bool Enabled { get; set; }
    public Dictionary<Language, Credentials> LanguageDict { get; set; } = new();
    public class Credentials
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string ConsumerKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string AccessTokenSecret { get; set; } = string.Empty;
    }
}