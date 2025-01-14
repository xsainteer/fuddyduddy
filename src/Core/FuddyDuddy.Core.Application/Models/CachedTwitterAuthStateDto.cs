using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Models;

public class CachedTwitterAuthStateDto
{
    public string CodeVerifier { get; set; } = string.Empty;
    public string CodeChallenge { get; set; } = string.Empty;
    public Language Language { get; set; }
}