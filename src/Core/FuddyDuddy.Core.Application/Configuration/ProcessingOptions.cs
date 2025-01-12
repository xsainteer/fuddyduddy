namespace FuddyDuddy.Core.Application.Configuration;

public class ProcessingOptions
{
    public const string SectionName = "Processing";
    public int DefaultCategoryId { get; set; }
    public string Country { get; set; }
    public string Currency { get; set; }
}