namespace FuddyDuddy.Core.Application.Configuration;

public class SearchSettings
{
    public const string SectionName = "Search";
    public bool Enabled { get; set; }
    public int MaxSearchResults { get; set; } = 10;
}