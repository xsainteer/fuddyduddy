namespace FuddyDuddy.Core.Application.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset ConvertToTimeZone(this DateTimeOffset dateTimeOffset, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, timeZone);
    }
}