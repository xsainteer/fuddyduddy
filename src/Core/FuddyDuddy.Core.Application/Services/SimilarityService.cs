using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
namespace FuddyDuddy.Core.Application.Services;

public interface ISimilarityService
{
    Task FindSimilarSummariesAsync(Guid newsSummaryId, CancellationToken cancellationToken);
}

public class SimilarityService : ISimilarityService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ISimilarRepository _similarRepository;
    private readonly IAiService _aiService;
    private readonly ILogger<SimilarityService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IOptions<SimilaritySettings> _similaritySettings;
    public SimilarityService(
        INewsSummaryRepository summaryRepository,
        ISimilarRepository similarRepository,
        IAiService aiService,
        ILogger<SimilarityService> logger,
        ICacheService cacheService,
        IOptions<SimilaritySettings> similaritySettings)
    {
        _summaryRepository = summaryRepository;
        _similarRepository = similarRepository;
        _aiService = aiService;
        _logger = logger;
        _cacheService = cacheService;
        _similaritySettings = similaritySettings;
    }

    public async Task FindSimilarSummariesAsync(Guid newsSummaryId, CancellationToken cancellationToken)
    {
        // Check if summary is already in a similarity group
        var existingSimilarGroups = await _similarRepository.GetBySummaryIdAsync(newsSummaryId, cancellationToken);
        if (existingSimilarGroups.Any())
        {
            _logger.LogInformation("Summary {SummaryId} is already in {Count} similarity groups", newsSummaryId, existingSimilarGroups.Count());
            return;
        }

        var newsSummary = await _summaryRepository.GetByIdAsync(newsSummaryId, cancellationToken);
        if (newsSummary == null)
        {
            _logger.LogError("News summary {SummaryId} not found", newsSummaryId);
            return;
        }

        // Get last 20 summaries from the last hour with the same language
        var recentSummaries = await _summaryRepository.GetByStateAsync(
            states: [NewsSummaryState.Created, NewsSummaryState.Validated, NewsSummaryState.Digested],
            dateTo: newsSummary.GeneratedAt,
            first: _similaritySettings.Value.MaxSimilarSummaries,
            categoryId: newsSummary.CategoryId,
            language: newsSummary.Language,
            cancellationToken: cancellationToken);

        var sameLangSummaries = recentSummaries
            .Where(s => s.Id != newsSummary.Id)
            .ToList();

        if (!sameLangSummaries.Any())
        {
            _logger.LogWarning("No recent summaries with same language and category found for comparison with {SummaryId}", newsSummary.Id);
            return;
        }

        // Find similar summaries using AI
        var systemPrompt = @$"You are a semantic similarity analyzer. Your task is to find a summary from the user input that is semantically similar/same to the source summary.
Language of summaries is {newsSummary.Language.GetDescription()}.

IMPORTANT EXCLUSION CRITERIA:
- If either source or candidate summary is a collection of unrelated news items (like 'daily news roundup' or 'Ежедневные новости'), skip it.

SIMILARITY CRITERIA (at least ONE must be strongly met):
1. Both summaries describe the same main event or announcement
2. Both summaries are direct updates or developments of the same story
3. Both summaries cover different aspects of the same specific incident

Consider connected_to_other_summaries field as supporting evidence - if summaries share connections, they're more likely to be related.

Return a JSON object with the following fields:
- similar_summary_id: the ID of the summary that is similar to the source summary
- reason: a short explanation of why the summary is similar (255 characters max)

If no summaries meet the criteria, return an empty object.
";


        var groupedSummaries = await _similarRepository.GetGroupedSummariesWithConnectedOnesAsync(
            numberOfLatestSimilars: _similaritySettings.Value.MaxSimilarSummaries,
            cancellationToken: cancellationToken);

        var summariesData = sameLangSummaries.Select(s => new SummaryComparisonData
        {
            Id = s.Id,
            Title = s.Title,
            Summary = s.Article[..Math.Min(512, s.Article.Length)],
            ConnectedToOtherSummaries = groupedSummaries.TryGetValue(s.Id, out var references) ? string.Join("; ", references.Select(r => r.Title)) : string.Empty
        }).ToList();

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var userInput = @$"Source summary: {JsonSerializer.Serialize(new SummaryComparisonData { Id = newsSummary.Id, Title = newsSummary.Title, Summary = newsSummary.Article }, jsonOptions)}

List of candidate summaries: {JsonSerializer.Serialize(summariesData, jsonOptions)}";

        var similarityResponse = await _aiService.GenerateStructuredResponseAsync<SimilarityResponse>(
            systemPrompt,
            userInput,
            new SimilarityResponse { SimilarSummaryId = Guid.NewGuid().ToString(), Reason = "Both summaries report on the same event. The close thematic and contextual overlap strongly suggests semantic similarity." },
            cancellationToken);

        if (similarityResponse?.SimilarSummaryId == null)
        {
            _logger.LogInformation("No similar summary found by AI for {SummaryId}", newsSummary.Id);
            return;
        }

        if (!Guid.TryParse(similarityResponse.SimilarSummaryId, out var similarSummaryId))
        {
            _logger.LogError("AI returned invalid similar summary ID {SimilarSummaryId} for {SummaryId}", similarityResponse.SimilarSummaryId, newsSummary.Id);
            return;
        }

        // Create similarity group
        var similarSummary = sameLangSummaries.FirstOrDefault(s => s.Id == similarSummaryId);
        if (similarSummary == null)
        {
            _logger.LogError("AI returned similar summary ID {SimilarSummaryId} but it was not found in the database requested for {SummaryId}", similarityResponse.SimilarSummaryId, newsSummary.Id);
            return;
        }

        var group = await _similarRepository.GetBySummaryIdAsync(similarSummary.Id, cancellationToken);
        if (group != null)
        {
            foreach (var item in group)
            {
                await _similarRepository.AddReferenceAsync(item, new SimilarReference(newsSummary.Id, similarityResponse.Reason), cancellationToken);
                _logger.LogInformation("Added reference {Title} to similarity group {Group}. Reason: {Reason}", newsSummary.Title, item.Title, similarityResponse.Reason);
                await AddSimilarReferencesToCacheAsync(item.References.ToList(), cancellationToken);
            }
        }
        else
        {
            var references = new List<SimilarReference>
            {
                new(newsSummary.Id, string.Empty),
                new(similarSummary.Id, similarityResponse.Reason)
            };
            await _similarRepository.AddAsync(new Similar(newsSummary.Title, newsSummary.Language, references), cancellationToken);
            _logger.LogInformation(
                "Created similarity group for {SummaryId} with name {Title} and similar summary {SimilarSummaryId} ({SimilarSummaryTitle}) with reason {Reason}",
                newsSummary.Id,
                newsSummary.Title,
                similarSummary.Id,
                similarSummary.Title,
                similarityResponse.Reason);
            await AddSimilarReferencesToCacheAsync(references, cancellationToken);
        }
    }

    private async Task AddSimilarReferencesToCacheAsync(List<SimilarReference> references, CancellationToken cancellationToken)
    {
        foreach (var reference in references)
        {
            await _cacheService.AddSummaryAsync(reference.NewsSummaryId, cancellationToken);
        }
    }
}

public class SummaryComparisonData
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
    [JsonPropertyName("connected_to_other_summaries")]
    public string ConnectedToOtherSummaries { get; set; } = string.Empty;
}