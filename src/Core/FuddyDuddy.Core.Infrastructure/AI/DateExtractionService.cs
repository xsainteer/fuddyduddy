using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.AI;
using Microsoft.Extensions.Logging;

namespace FuddyDuddy.Core.Infrastructure.AI;

internal class DateExtractionService : IDateExtractionService
{
    private readonly IAiService _aiService;
    private readonly ILogger<DateExtractionService> _logger;

    private const string SYSTEM_PROMPT = @"
        Extract dates from user's Question and return them in yyyy-MM-dd format.
        For single date, use same value for both fields, eg.: вчера [from: yesterday, to: yesterday]; позавчера [from: two days ago, to: two days ago].
        For date ranges, set both fields eg.: в январе [from: 2025-01-01, to: 2025-01-31].
        Return JSON with 'from' and 'to' fields.
        IMPORTANT: Today's information will be given in the user's prompt.
        ";

    private const string USER_PROMPT_INFO = """
        Current info: today is {0}, day of month is {1}, month is {2}, weekday is {3}.
        Yesterday is {4}, two days ago is {5}.
        """;

    public DateExtractionService(IAiService aiService, ILogger<DateExtractionService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<DateRangeResponse> ExtractDateRangeAsync(string text, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting date range from text: {Text}", text);
        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1).Date.ToString("yyyy-MM-dd");
        var twoDaysAgo = now.AddDays(-2).Date.ToString("yyyy-MM-dd");
        var today = now.Date.ToString("yyyy-MM-dd");
        var response = await _aiService.GenerateStructuredResponseAsync(
            string.Format(SYSTEM_PROMPT, today),
            $@"{string.Format(USER_PROMPT_INFO, today, now.Day, now.Month, now.DayOfWeek, yesterday, twoDaysAgo)}
            Question: {text}",
            new DateRangeResponse { From = "2025-01-13", To = "2025-01-13" },
            cancellationToken
        );

        return response ?? new DateRangeResponse { From = null, To = null };
    }
} 