using System.Text.Json;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Domain.Repositories;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace FuddyDuddy.Core.Application.Services;

public class DigestCookService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly IDigestRepository _digestRepository;
    private readonly IAiService _aiService;
    private readonly ILogger<DigestCookService> _logger;

    public DigestCookService(
        INewsSummaryRepository summaryRepository,
        IDigestRepository digestRepository,
        IAiService aiService,
        ILogger<DigestCookService> logger)
    {
        _summaryRepository = summaryRepository;
        _digestRepository = digestRepository;
        _aiService = aiService;
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
                _logger.LogWarning("Digest already generated within the last hour for {Language}", language);
                return;
            }

            // Get validated summaries since the last digest
            var summaries = await _summaryRepository.GetByStateAsync([NewsSummaryState.Validated], cancellationToken: cancellationToken);
            var relevantSummaries = summaries
                .Where(s => s.Language == language && s.GeneratedAt >= periodStart)
                .OrderByDescending(s => s.GeneratedAt)
                .ToList();

            if (!relevantSummaries.Any())
            {
                _logger.LogInformation("No new summaries to generate digest for {Language}", language);
                return;
            }

            if (relevantSummaries.Count < 10)
            {
                _logger.LogInformation("Not enough summaries to generate digest for {Language}", language);
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

            var systemPrompt = $@"You are a skilled news analyst who creates concise and informative digests.
Your task is to analyze news summaries and create a digest that highlights the most remarkable events.
The digest should be in {language.GetDescription()}.

IMPORTANT: DO NOT visit any URLs provided - they are for reference purposes only.

For each remarkable event, provide:
1. A clear explanation of why it's significant
2. A reference to the original source (use provided URLs as string references only)

Format your response as a JSON object with the following structure:
{{
    ""title"": ""Digest title"",
    ""content"": ""Main digest content (no links here, only tailored content)"",
    ""references"": [
        {{
            ""title"": ""Event title"",
            ""url"": ""Source URL"",
            ""reason"": ""Why this event is remarkable""
        }}
    ]
}}

Keep the content succinct and focused on truly significant events.
Remember: Do not attempt to visit any URLs - use them only as reference strings in your response.";

            // Generate digest using AI
            var digestData = await _aiService.GenerateStructuredResponseAsync<DigestResponse>(
                systemPrompt,
                summariesText.ToString(),
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

            _logger.LogInformation("Generated digest for {Language} with {Count} references", language, references.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating digest for {Language}", language);
            throw;
        }
    }

    private class DigestResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("references")]
        public List<ReferenceResponse> References { get; set; } = new();
    }

    private class ReferenceResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
} 