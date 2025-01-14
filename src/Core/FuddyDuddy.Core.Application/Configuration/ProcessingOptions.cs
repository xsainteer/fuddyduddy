namespace FuddyDuddy.Core.Application.Configuration;

public class ProcessingOptions
{
    public const string SectionName = "Processing";
    public int DefaultCategoryId { get; set; }
    public string Country { get; set; }
    public string CountrySpell { get; set; } = string.Empty;
    public string Currency { get; set; }
    public string Timezone { get; set; }
    public string TweetPostHours { get; set; } = string.Empty;
    public int MaxTweetLength { get; set; }
    public string Domain { get; set; } = string.Empty;
    public int[] TweetPostHoursList => TweetPostHours.Split('-').Select(int.Parse).ToArray();

    public int TweetPostMinutesBack { get; set; } = 60;
}
