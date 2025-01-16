using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.AI;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace FuddyDuddy.Core.Application.Services;

public interface ISimilarityService
{
    Task CheckForSimilarSummariesAsync(CancellationToken cancellationToken);
    Task FindSimilarSummariesAsync(NewsSummary newsSummary, CancellationToken cancellationToken);
}

public class SimilarityService : ISimilarityService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ISimilarRepository _similarRepository;
    private readonly IAiService _aiService;
    private readonly ILogger<SimilarityService> _logger;
    private readonly ICacheService _cacheService;

    public SimilarityService(
        INewsSummaryRepository summaryRepository,
        ISimilarRepository similarRepository,
        IAiService aiService,
        ILogger<SimilarityService> logger,
        ICacheService cacheService)
    {
        _summaryRepository = summaryRepository;
        _similarRepository = similarRepository;
        _aiService = aiService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task CheckForSimilarSummariesAsync(CancellationToken cancellationToken)
    {
        var summaries = (await _summaryRepository.GetByStateAsync([NewsSummaryState.Created], cancellationToken: cancellationToken))
            .OrderByDescending(s => s.GeneratedAt);

        foreach (var summary in summaries)
        {
            try
            {
                await FindSimilarSummariesAsync(summary, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar summaries for {SummaryId}", summary.Id);
            }
        }
    }

    public async Task FindSimilarSummariesAsync(NewsSummary newsSummary, CancellationToken cancellationToken)
    {
        // Check if summary is already in a similarity group
        var existingSimilarGroups = await _similarRepository.GetBySummaryIdAsync(newsSummary.Id, cancellationToken);
        if (existingSimilarGroups.Any())
        {
            _logger.LogInformation("Summary {SummaryId} is already in {Count} similarity groups", newsSummary.Id, existingSimilarGroups.Count());
            return;
        }

        // Get summaries from the last hour with the same language
        var oneHourAgo = DateTimeOffset.UtcNow.AddHours(-1);
        var recentSummaries = await _summaryRepository.GetByStateAsync(
            [NewsSummaryState.Created, NewsSummaryState.Validated, NewsSummaryState.Digested],
            oneHourAgo,
            cancellationToken);

        var sameLangSummaries = recentSummaries
            .Where(s => s.Language == newsSummary.Language && s.CategoryId == newsSummary.CategoryId && s.Id != newsSummary.Id)
            .ToList();

        if (!sameLangSummaries.Any())
        {
            _logger.LogInformation("No recent summaries with same language and category found for comparison with {SummaryId}", newsSummary.Id);
            return;
        }

        // Prepare data for AI analysis
        var summaryData = new List<SummaryComparisonData>
        {
            new() { Id = newsSummary.Id, Title = newsSummary.Title, Summary = newsSummary.Article[..Math.Min(200, newsSummary.Article.Length)] }
        };

        summaryData.AddRange(sameLangSummaries.Select(s => new SummaryComparisonData
        {
            Id = s.Id,
            Title = s.Title,
            Summary = s.Article[..Math.Min(200, s.Article.Length)]
        }));

        // Find similar summaries using AI
        var systemPrompt = @"You are a semantic similarity analyzer. Your task is to find a summary that is semantically STRONGLY similar to the first summary in the list.
Two summaries are considered similar if they:
1. Cover the same event or closely related events
2. Share significant contextual overlap
3. Are part of the same ongoing story

Return a JSON object with the following properties:
- similar_summary_id: the ID of the summary that is 100% similar to the first summary
- reason: a short explanation of why the summary is similar to the first summary

If no summaries are similar enough, return an empty object.
It's better to return none than a SOMEWHAT similar summary.
So be very strict in your similarity criteria.";

        var similarityResponse = await _aiService.GenerateStructuredResponseAsync<SimilarityResponse>(
            systemPrompt,
            System.Text.Json.JsonSerializer.Serialize(summaryData),
            new SimilarityResponse(),
            cancellationToken);

        if (similarityResponse?.SimilarSummaryId == null)
        {
            _logger.LogInformation("No similar summary found by AI for {SummaryId}", newsSummary.Id);
            return;
        }

        // Create similarity group
        var similarSummary = sameLangSummaries.FirstOrDefault(s => s.Id == similarityResponse.SimilarSummaryId);
        if (similarSummary == null)
        {
            _logger.LogError("AI returned similar summary ID {SimilarSummaryId} but it was not found in the database requested for {SummaryId}", similarityResponse.SimilarSummaryId, newsSummary.Id);
            return;
        }

        var group = await _similarRepository.GetBySummaryIdAsync(similarSummary.Id, cancellationToken);
        if (group != null && group.Any())
        {
            foreach (var item in group)
            {
                await _similarRepository.AddReferenceAsync(item, new SimilarReference(newsSummary.Id, similarityResponse.Reason), cancellationToken);
                _logger.LogInformation("Added reference {Title} to similarity group {Group}. Reason: {Reason}", newsSummary.Title, item.Title, similarityResponse.Reason);
            }
        }
        else
        {
            await _similarRepository.AddAsync(new Similar(newsSummary.Title, newsSummary.Language, new() { new(newsSummary.Id, similarityResponse.Reason) }), cancellationToken);
            _logger.LogInformation(
                "Created similarity group for {SummaryId} with name {Title} and similar summary {SimilarSummaryId} ({SimilarSummaryTitle}) with reason {Reason}",
                newsSummary.Id,
                newsSummary.Title,
                similarSummary.Id,
                similarSummary.Title,
                similarityResponse.Reason);
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
}