using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class TwitterOptions
{
    public Dictionary<Language, Details> LanguageDict { get; set; } = new();

    public class Details
    {
        public string BearerToken { get; set; } = string.Empty;
    }
}