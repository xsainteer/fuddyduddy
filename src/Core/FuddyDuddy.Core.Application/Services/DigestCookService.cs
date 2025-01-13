using System.Text.Json;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Configuration;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Models;
using FuddyDuddy.Core.Application.Extensions;

namespace FuddyDuddy.Core.Application.Services;

public interface IDigestCookService
{
    Task GenerateDigestAsync(Language language, CancellationToken cancellationToken = default);
    Task<bool> GenerateTweetAsync(Language language, CancellationToken cancellationToken = default);
}

internal class DigestCookService : IDigestCookService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly IDigestRepository _digestRepository;
    private readonly IAiService _aiService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DigestCookService> _logger;
    private readonly IOptions<ProcessingOptions> _processingOptions;
    private readonly ITwitterConnectorFactory _twitterConnectorFactory;

    public DigestCookService(
        INewsSummaryRepository summaryRepository,
        IDigestRepository digestRepository,
        IAiService aiService,
        ICacheService cacheService,
        ITwitterConnectorFactory twitterConnectorFactory,
        ILogger<DigestCookService> logger,
        IOptions<ProcessingOptions> processingOptions)
    {
        _summaryRepository = summaryRepository;
        _digestRepository = digestRepository;
        _aiService = aiService;
        _cacheService = cacheService;
        _twitterConnectorFactory = twitterConnectorFactory;
        _logger = logger;
        _processingOptions = processingOptions;
    }

    public async Task GenerateDigestAsync(Language language, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the latest digest for the language to determine the start period
            var lastDigest = await _digestRepository.GetLatestByLanguageAsync(language, cancellationToken);
            var periodStart = lastDigest?.PeriodEnd ?? DateTimeOffset.UtcNow.AddHours(-12);
            var periodEnd = DateTimeOffset.UtcNow;

            if (lastDigest != null && Math.Abs((DateTimeOffset.UtcNow - lastDigest.GeneratedAt).TotalHours) < 1)
            {
                _logger.LogInformation("Digest already generated within the last hour for {Language}", language);
                return;
            }

            // Get validated summaries since the last digest
            var summaries = await _summaryRepository.GetByStateAsync([NewsSummaryState.Validated], cancellationToken: cancellationToken);
            var relevantSummaries = summaries
                .Where(s => s.Language == language && s.GeneratedAt >= periodStart)
                .OrderByDescending(s => s.GeneratedAt)
                .ToList();

            // If no relevant summaries, generate an empty digest anyway
            if (!relevantSummaries.Any())
            {
                _logger.LogInformation("No new summaries to generate digest for {Language}. Generating an empty digest.", language);
                var emptyDigest = new Digest(
                    language == Language.RU ? "Пустой дайджест" : "Empty digest",
                    language == Language.RU ? "Событий нет за прошедший час, но, возможно появятся в ближайшее время! Оставайтесь на связи!" : "No events in the last hour, but new events may appear soon! Stay tuned!",
                    language,
                    periodStart,
                    periodEnd,
                    new List<DigestReference>(),
                    DigestState.Published);

                await _digestRepository.AddAsync(emptyDigest, cancellationToken);
                await _cacheService.AddDigestAsync(CachedDigestDto.FromDigest(emptyDigest), cancellationToken);

                return;
            }

            // Format summaries as plain text
            var summariesText = new StringBuilder();
            foreach (var summary in relevantSummaries)
            {
                summariesText.AppendLine($"Time: {summary.GeneratedAt.ConvertToTimeZone(_processingOptions.Value.Timezone):HH:mm}");
                summariesText.AppendLine($"Title: {summary.Title}");
                summariesText.AppendLine($"Article: {summary.Article}");
                summariesText.AppendLine($"URL: {summary.NewsArticle.Url} (DO NOT VISIT - reference only)");
                summariesText.AppendLine();
            }

            var sample = new DigestResponse
            {
                Title = "Digest title for last hour",
                Content = "Main digest content (no links here, only tailored content)",
                References = new List<ReferenceResponse>
                {
                    new ReferenceResponse { Title = "Event title", Url = "Source URL", Reason = "Why this event is remarkable" }
                }
            };

            var systemPrompt = $@"You are a skilled news analyst from {_processingOptions.Value.Country} who creates concise and informative digests.
Your task is to analyze news summaries and create a digest that highlights the most remarkable events.
The digest should be in {language.GetDescription()}.

IMPORTANT: DO NOT visit any URLs provided - they are for reference purposes only.

For each remarkable event, provide:
1. A clear explanation of why it's significant
2. A reference to the original source (use provided URLs as string references only)

Keep the content succinct and focused on truly significant events.
Remember: Do not attempt to visit any URLs - use them only as reference strings in your response.
The currency in {_processingOptions.Value.Country} is {_processingOptions.Value.Currency}.";

            // Generate digest using AI
            var digestData = await _aiService.GenerateStructuredResponseAsync<DigestResponse>(
                systemPrompt,
                summariesText.ToString(),
                sample,
                cancellationToken);

            if (digestData == null)
            {
                _logger.LogError("Failed to generate digest");
                return;
            }

            _logger.LogInformation("Generated digest data: {DigestData}", JsonSerializer.Serialize(digestData));

            // Create references for remarkable events
            var references = digestData
                .References
                .Where(r => relevantSummaries.Any(s => s.NewsArticle.Url == r.Url))
                .Select(r => new DigestReference(
                    relevantSummaries.First(s => s.NewsArticle.Url == r.Url).Id,
                    r.Title,
                    r.Url,
                    r.Reason
                ))
                .DistinctBy(r => (r.NewsSummaryId, r.DigestId))
                .ToList();

            // Create and save the digest
            var digest = new Digest(
                digestData.Title,
                digestData.Content,
                language,
                periodStart,
                periodEnd,
                references,
                DigestState.Published);

            await _digestRepository.AddAsync(digest, cancellationToken);

            // Mark summarized news as digested
            foreach (var summary in relevantSummaries)
            {
                summary.MarkAsDigested();
                await _summaryRepository.UpdateAsync(summary, cancellationToken);
            }

            var savedDigest = await _digestRepository.GetByIdAsync(digest.Id, cancellationToken);

            if (savedDigest == null)
            {
                _logger.LogError("Failed to get saved digest");
                return;
            }

            // Cache the new digest
            await _cacheService.AddDigestAsync(CachedDigestDto.FromDigest(savedDigest), cancellationToken);

            _logger.LogInformation("Generated and cached digest for {Language} with {Count} references", language, references.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating digest for {Language}", language);
            throw;
        }
    }

    public async Task<bool> GenerateTweetAsync(Language language, CancellationToken cancellationToken = default)
    {
        try
        {
            var hours = _processingOptions.Value.TweetPostHoursList;
            var currentHour = DateTimeOffset.UtcNow.Hour;

            if (hours.Length == 2 && (currentHour < hours[0] || currentHour > hours[1]))
            {
                _logger.LogInformation("It's not time to tweet yet. Allowed hours: {Hours}", string.Join("-", hours));
                return false;
            }

            // Get the timestamp of the last tweet
            var lastTweetTimestamp = await _cacheService.GetLastTweetTimestampAsync(language, cancellationToken)
                ?? DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();

            var lastTweetTime = DateTimeOffset.FromUnixTimeSeconds(lastTweetTimestamp);
            var currentTime = DateTimeOffset.UtcNow;

            // Check if 2 hours have passed since the last tweet
            if (currentTime - lastTweetTime < TimeSpan.FromHours(1))
            {
                _logger.LogInformation("Not enough time passed since last tweet at {LastTweetTime}", lastTweetTime);
                return false;
            }

            // Get digests since last tweet
            var digests = await _digestRepository.GetLatestAsync(language, lastTweetTime, cancellationToken);
            var relevantDigests = digests
                .Where(d => d.State == DigestState.Published)
                .Where(d => d.References.Any(r => r.NewsSummary.Language == language))
                .OrderByDescending(d => d.GeneratedAt)
                .ToList();

            _logger.LogInformation("Found {Count} relevant digests", relevantDigests.Count);

            if (relevantDigests.Count == 0)
            {
                _logger.LogInformation("No new digests to tweet about since {LastTweetTime}", lastTweetTime);
                return false;
            }

            // Format digests content for AI
            var digestsText = new StringBuilder();
            foreach (var digest in relevantDigests)
            {
                digestsText.AppendLine($"Time: {digest.GeneratedAt.ConvertToTimeZone(_processingOptions.Value.Timezone):HH:mm}");
                digestsText.AppendLine($"Content: {digest.Content}");
                digestsText.AppendLine();
            }

            var sample = new TweetCreationResponse
            {
                Tweet = "Just-in in KG. Extremely unhealthy air quality in Bishkek posing significant health risks to residents. Meanwhile, the Alatau Reservoir offers successful fishing opportunities for anglers catching large perch."
            };

            var systemPrompt = $@"You are a social media expert crafting engaging tweets about {_processingOptions.Value.Country} news.
Create a tweet that:
1. Highlights the most-most important news from the provided digests (because of the length constraint of 280 characters)
2. Feel free to rephrase the news to make it more engaging and succinct (because of the length constraint of 280 characters)
3. Uses engaging but professional language
4. Includes the provided URL
5. MUST be under 280 characters (including the URL and hashtag)
6. Maintains journalistic integrity
7. Includes the URL: https://{_processingOptions.Value.Domain}/{language.GetDescription().ToLower()}/digests
8. add just one hashtag: #kgnews

IMPORTANT: if you think there is no news to tweet about, just return an empty string.

Remember: The goal is to inform and engage while being concise and professional.";

            // Generate tweet using AI
            var tweetData = await _aiService.GenerateStructuredResponseAsync<TweetCreationResponse>(
                systemPrompt,
                digestsText.ToString(),
                sample,
                cancellationToken);

            if (string.IsNullOrEmpty(tweetData?.Tweet))
            {
                _logger.LogError("Failed to generate tweet");
                return false;
            }

            if (tweetData.Tweet.Length > _processingOptions.Value.MaxTweetLength)
            {
                _logger.LogError("Tweet is too long: {TweetLength}. Tweet: {Tweet}", tweetData.Tweet.Length, tweetData.Tweet);
                return false;
            }

            // Post tweet
            var twitterConnector = _twitterConnectorFactory.Create(language);
            var tweetId = await twitterConnector.PostTweetAsync(tweetData.Tweet);

            if (tweetId == null)
            {
                _logger.LogError("Failed to post tweet: {Tweet}", tweetData.Tweet);
                return false;
            }

            _logger.LogInformation("Tweet posted successfully: {TweetId}", tweetId);

            // Update last tweet timestamp
            await _cacheService.SetLastTweetTimestampAsync(language, currentTime.ToUnixTimeSeconds(), cancellationToken);

            _logger.LogInformation("Tweet posted successfully: {Tweet}", tweetData.Tweet);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tweet");
            return false;
        }
    }
} 