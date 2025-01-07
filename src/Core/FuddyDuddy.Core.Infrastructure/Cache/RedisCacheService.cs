using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace FuddyDuddy.Core.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private const string SUMMARY_KEY = "summary:{0}";  // Individual summary
    private const string SUMMARIES_BY_LANGUAGE_KEY = "latest:summaries:{0}";  // Timeline by language
    private const string SUMMARIES_BY_CATEGORY_KEY = "summaries:by:category:{0}";  // Category index
    private const string SUMMARIES_BY_SOURCE_KEY = "summaries:by:source:{0}";  // Source index
    private const int MAX_SUMMARIES = 1000;  // Per language

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task AddSummaryAsync(NewsSummary summary, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var score = summary.GeneratedAt.ToUnixTimeSeconds();
            var cachedSummary = CachedSummaryDto.FromNewsSummary(summary);
            var summaryJson = JsonSerializer.Serialize(cachedSummary);
            
            // Store the summary itself
            await db.StringSetAsync(
                string.Format(SUMMARY_KEY, summary.Id),
                summaryJson,
                TimeSpan.FromDays(7));  // TTL for individual summaries

            // Add to the language-specific timeline
            var timelineKey = string.Format(SUMMARIES_BY_LANGUAGE_KEY, summary.Language.ToString().ToLower());
            await db.SortedSetAddAsync(timelineKey, summaryJson, score);

            // Add to category index
            await db.SetAddAsync(
                string.Format(SUMMARIES_BY_CATEGORY_KEY, summary.CategoryId),
                summary.Id.ToString());

            // Add to source index
            await db.SetAddAsync(
                string.Format(SUMMARIES_BY_SOURCE_KEY, summary.NewsArticle.NewsSourceId),
                summary.Id.ToString());

            // Trim the language-specific timeline
            await db.SortedSetRemoveRangeByRankAsync(timelineKey, 0, -MAX_SUMMARIES - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding summary to cache");
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetLatestSummariesAsync<T>(
        int skip,
        int take,
        Language language = Language.RU,
        int? categoryId = null,
        Guid? sourceId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var timelineKey = string.Format(SUMMARIES_BY_LANGUAGE_KEY, language.ToString().ToLower());

            // If no filters, just return from timeline
            if (categoryId == null && sourceId == null)
            {
                var summaries = await db.SortedSetRangeByRankAsync(
                    timelineKey,
                    skip,
                    skip + take - 1,
                    Order.Descending);

                return DeserializeSummaries<T>(summaries);
            }

            // Get filtered IDs
            var filteredIds = await GetFilteredSummaryIdsAsync(db, categoryId, sourceId);
            if (!filteredIds.Any())
                return Enumerable.Empty<T>();

            // Get scores for filtered IDs from timeline
            var summaryScores = await db.SortedSetRangeByRankWithScoresAsync(timelineKey);
            var filteredSummaries = summaryScores
                .Where(x => filteredIds.Contains(GetSummaryId(x.Element)))
                .Skip(skip)
                .Take(take)
                .Select(x => x.Element)
                .ToArray();  // Convert to array for DeserializeSummaries

            return DeserializeSummaries<T>(filteredSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest summaries");
            return Enumerable.Empty<T>();
        }
    }

    public async Task<T?> GetSummaryByIdAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var summaryJson = await db.StringGetAsync(string.Format(SUMMARY_KEY, id));
            
            if (summaryJson.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(summaryJson!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary by ID");
            return default;
        }
    }

    public async Task CacheSummaryDtoAsync<T>(string id, T summary, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var summaryJson = JsonSerializer.Serialize(summary);
            
            await db.StringSetAsync(
                string.Format(SUMMARY_KEY, id),
                summaryJson,
                TimeSpan.FromDays(7));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching summary DTO");
            throw;
        }
    }

    public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            // Clear all language-specific timelines
            foreach (var language in Enum.GetValues<Language>())
            {
                var timelineKey = string.Format(SUMMARIES_BY_LANGUAGE_KEY, language.ToString().ToLower());
                await db.KeyDeleteAsync(timelineKey);
            }

            // Clear all summary keys
            var summaryKeys = server.Keys(pattern: "summary:*");
            foreach (var key in summaryKeys)
            {
                await db.KeyDeleteAsync(key);
            }

            // Clear all category indexes
            var categoryKeys = server.Keys(pattern: "summaries:by:category:*");
            foreach (var key in categoryKeys)
            {
                await db.KeyDeleteAsync(key);
            }

            // Clear all source indexes
            var sourceKeys = server.Keys(pattern: "summaries:by:source:*");
            foreach (var key in sourceKeys)
            {
                await db.KeyDeleteAsync(key);
            }

            _logger.LogInformation("Cache cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            throw;
        }
    }

    private async Task<HashSet<string>> GetFilteredSummaryIdsAsync(
        IDatabase db,
        int? categoryId = null,
        Guid? sourceId = null)
    {
        var sets = new List<RedisKey>();

        if (categoryId.HasValue)
            sets.Add(string.Format(SUMMARIES_BY_CATEGORY_KEY, categoryId.Value));

        if (sourceId.HasValue)
            sets.Add(string.Format(SUMMARIES_BY_SOURCE_KEY, sourceId.Value));

        if (!sets.Any())
            return new HashSet<string>();

        // Intersect all filter sets
        var intersection = await db.SetCombineAsync(SetOperation.Intersect, sets.ToArray());
        return intersection.Select(x => x.ToString()).ToHashSet();
    }

    private string GetSummaryId(RedisValue summaryJson)
    {
        try
        {
            var summary = JsonSerializer.Deserialize<CachedSummaryDto>(summaryJson!);
            return summary?.Id.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private IEnumerable<T> DeserializeSummaries<T>(RedisValue[] summaries)
    {
        return summaries
            .Select(s => JsonSerializer.Deserialize<T>(s!))
            .Where(s => s != null)
            .Cast<T>();
    }
} 