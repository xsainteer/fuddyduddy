using System.Text.Json;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Models;

namespace FuddyDuddy.Core.Application.Services;

public interface IDigestCookService
{
    Task GenerateDigestAsync(Language language, CancellationToken cancellationToken = default);
}

internal class DigestCookService : IDigestCookService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly IDigestRepository _digestRepository;
    private readonly IGeminiService _aiService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DigestCookService> _logger;

    public DigestCookService(
        INewsSummaryRepository summaryRepository,
        IDigestRepository digestRepository,
        IGeminiService aiService,
        ICacheService cacheService,
        ILogger<DigestCookService> logger)
    {
        _summaryRepository = summaryRepository;
        _digestRepository = digestRepository;
        _aiService = aiService;
        _cacheService = cacheService;
        _logger = logger;
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
                summariesText.AppendLine($"Time: {summary.GeneratedAt:HH:mm}");
                summariesText.AppendLine($"Title: {summary.Title}");
                summariesText.AppendLine($"Article: {summary.Article}");
                summariesText.AppendLine($"URL: {summary.NewsArticle.Url} (DO NOT VISIT - reference only)");
                summariesText.AppendLine();
            }

            var sample = new DigestResponse
            {
                Title = "Digest title",
                Content = "Main digest content (no links here, only tailored content)",
                References = new List<ReferenceResponse>
                {
                    new ReferenceResponse { Title = "Event title", Url = "Source URL", Reason = "Why this event is remarkable" }
                }
            };

            var systemPrompt = $@"You are a skilled news analyst who creates concise and informative digests.
Your task is to analyze news summaries and create a digest that highlights the most remarkable events.
The digest should be in {language.GetDescription()}.

IMPORTANT: DO NOT visit any URLs provided - they are for reference purposes only.

For each remarkable event, provide:
1. A clear explanation of why it's significant
2. A reference to the original source (use provided URLs as string references only)

Keep the content succinct and focused on truly significant events.
Remember: Do not attempt to visit any URLs - use them only as reference strings in your response.";

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
                )).ToList();

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
} 