namespace FuddyDuddy.Api.Configuration;

public class TaskSchedulerSettings
{
    public bool Enabled { get; set; } = false;
    public bool SummaryTask { get; set; } = false;
    public bool ValidationTask { get; set; } = false;
    public bool TranslationTask { get; set; } = false;
    public bool DigestTask { get; set; } = false;
    public string DigestTaskInactivityHoursRange { get; set; } = "16,01";
    public TimeSpan SummaryPipelineInterval { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan DigestPipelineInterval { get; set; } = TimeSpan.FromMinutes(30);

    public (int, int) DigestTaskInactivityHoursRangeTuple => (int.Parse(DigestTaskInactivityHoursRange.Split(',')[0]), int.Parse(DigestTaskInactivityHoursRange.Split(',')[1]));
}