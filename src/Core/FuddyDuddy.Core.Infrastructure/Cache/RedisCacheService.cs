using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Domain.Entities;
using FuddyDuddy.Core.Application.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using FuddyDuddy.Core.Application.Repositories;
using System.Linq;

namespace FuddyDuddy.Core.Infrastructure.Cache;

internal class RedisCacheService : ICacheService
{
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ISimilarRepository _similarRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private const string SUMMARY_KEY = "summary:{0}";  // Individual summary
    private const string SUMMARIES_BY_LANGUAGE_KEY = "latest:summaries:{0}";  // Timeline by language
    private const string SUMMARIES_BY_CATEGORY_KEY = "summaries:by:category:{0}";  // Category index
    private const string SUMMARIES_BY_SOURCE_KEY = "summaries:by:source:{0}";  // Source index
    private const string SUMMARY_LOCK_KEY = "summaries:lock:{0}";
    private const int MAX_SUMMARIES = 1000;  // Per language

    // New constants for digest caching
    private const string DIGEST_KEY = "digest:{0}";  // Individual digest
    private const string DIGESTS_BY_LANGUAGE_KEY = "latest:digests:{0}";  // Timeline by language
    private const int MAX_DIGESTS = 100;  // Per language

    // Digest tweet timestamp
    private const string LAST_TWEET_KEY = "lastTweetTimestamp:{0}";
    private const string TWITTER_TOKEN_KEY = "twitter:token:{0}"; // {0} is language

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger,
        INewsSummaryRepository summaryRepository,
        ISimilarRepository similarRepository)
    {
        _redis = redis;
        _logger = logger;
        _summaryRepository = summaryRepository;
        _similarRepository = similarRepository;
    }

    public async Task<string?> ExecuteLuaAsync(string script, string[] keys, string[] values)
    {
        var redisKeys = keys.Select(k => new RedisKey(k)).ToArray();
        var redisValues = values.Select(v => new RedisValue(v)).ToArray();
        var db = _redis.GetDatabase();
        var result = await db.ScriptEvaluateAsync(script, redisKeys, redisValues);
        return result.IsNull ? null : result.ToString();
    }

    public async Task AddSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default)
    {
        await ExecuteWithLock(
            string.Format(SUMMARY_LOCK_KEY, summaryId),
            async () => await AddOrUpdateSummaryAsync(summaryId, cancellationToken));
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
            var summaryScores = await db.SortedSetRangeByRankWithScoresAsync(timelineKey, 0, -1, Order.Descending);
            var filteredSummaries = summaryScores
                .Where(x => filteredIds.Contains(GetSummaryId(x.Element)))
                .Skip(skip)
                .Take(take)
                .Select(x => x.Element)
                .ToArray();

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

    public async Task AddDigestAsync(CachedDigestDto digest, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var score = digest.GeneratedAt.ToUnixTimeSeconds();
            var digestJson = JsonSerializer.Serialize(digest);
            
            // Store the digest itself
            await db.StringSetAsync(
                string.Format(DIGEST_KEY, digest.Id),
                digestJson,
                TimeSpan.FromDays(7));  // TTL for individual digests

            // Add to the language-specific timeline
            var timelineKey = string.Format(DIGESTS_BY_LANGUAGE_KEY, digest.Language.ToString().ToLower());
            await db.SortedSetAddAsync(timelineKey, digestJson, score);

            // Trim the language-specific timeline
            await db.SortedSetRemoveRangeByRankAsync(timelineKey, 0, -MAX_DIGESTS - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding digest to cache");
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetLatestDigestsAsync<T>(
        int skip,
        int take,
        Language language = Language.RU,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var timelineKey = string.Format(DIGESTS_BY_LANGUAGE_KEY, language.ToString().ToLower());

            var digests = await db.SortedSetRangeByRankAsync(
                timelineKey,
                skip,
                skip + take - 1,
                Order.Descending);

            return DeserializeDigests<T>(digests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest digests from cache");
            return Enumerable.Empty<T>();
        }
    }

    public async Task<T?> GetDigestByIdAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var digestJson = await db.StringGetAsync(string.Format(DIGEST_KEY, id));
            
            if (digestJson.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(digestJson!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting digest by ID from cache");
            return default;
        }
    }

    public async Task CacheDigestDtoAsync<T>(string id, T digest, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var digestJson = JsonSerializer.Serialize(digest);
            
            await db.StringSetAsync(
                string.Format(DIGEST_KEY, id),
                digestJson,
                TimeSpan.FromDays(7));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching digest DTO");
            throw;
        }
    }

    public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            // Clear all language-specific timelines (summaries and digests)
            foreach (var language in Enum.GetValues<Language>())
            {
                var summaryTimelineKey = string.Format(SUMMARIES_BY_LANGUAGE_KEY, language.ToString().ToLower());
                var digestTimelineKey = string.Format(DIGESTS_BY_LANGUAGE_KEY, language.ToString().ToLower());
                await db.KeyDeleteAsync(summaryTimelineKey);
                await db.KeyDeleteAsync(digestTimelineKey);
            }

            // Clear all summary and digest keys
            var summaryKeys = server.Keys(pattern: "summary:*");
            var digestKeys = server.Keys(pattern: "digest:*");
            foreach (var key in summaryKeys.Concat(digestKeys))
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

    public async Task<long?> GetLastTweetTimestampAsync(Language language, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = string.Format(LAST_TWEET_KEY, language.ToString().ToLower());
        var timestamp = await db.StringGetAsync(key);
        return long.TryParse(timestamp, out var result) ? result : null;
    }

    public async Task SetLastTweetTimestampAsync(Language language, long timestamp, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = string.Format(LAST_TWEET_KEY, language.ToString().ToLower());
        await db.StringSetAsync(key, timestamp.ToString());
    }

    public async Task<string?> GetTwitterTokenAsync(Language language, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = string.Format(TWITTER_TOKEN_KEY, language.ToString().ToLower());
        var redisValue = await db.StringGetAsync(key);
        return redisValue == RedisValue.Null ? null : redisValue.ToString();
    }

    public async Task SetTwitterTokenAsync(Language language, string token, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = string.Format(TWITTER_TOKEN_KEY, language.ToString().ToLower());
        await db.StringSetAsync(key, token, expiration);
    }

    public async Task ExecuteWithLock(string key, Func<Task> action, TimeSpan? expiration = null)
    {
        var db = _redis.GetDatabase();
        await db.LockTakeAsync(key, "lock", expiration ?? TimeSpan.FromSeconds(10));
        try
        {
            await action();
        }
        finally
        {
            await db.LockReleaseAsync(key, "lock");
        }
    }

    private async Task AddOrUpdateSummaryAsync(Guid summaryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _summaryRepository.GetByIdAsync(summaryId, cancellationToken);
            if (summary == null)
            {
                _logger.LogError("Failed to add summary {Id} to cache. Summary not found in repository.", summaryId);
                return;
            }
            var score = summary.GeneratedAt.ToUnixTimeSeconds();

            var cachedSummary = CachedSummaryDto.FromNewsSummary(summary);
            var similar = await _similarRepository.GetBySummaryIdAsync(summary.Id, cancellationToken);
            if (similar != null)
            {
                foreach (var s in similar)
                {
                    cachedSummary.AddBaseSimilarities(s);
                }
            }
            
            // Store the summary itself
            var summaryJson = JsonSerializer.Serialize(cachedSummary);
            var db = _redis.GetDatabase();
            await db.StringSetAsync(
                string.Format(SUMMARY_KEY, summary.Id),
                summaryJson,
                TimeSpan.FromDays(7));  // TTL for individual summaries

            // Add to the language-specific timeline
            var timelineKey = string.Format(SUMMARIES_BY_LANGUAGE_KEY, summary.Language.ToString().ToLower());
            
            // Check for existing entry with the same score (same GeneratedAt)
            var existingEntries = await db.SortedSetRangeByScoreAsync(timelineKey, score, score);
            if (existingEntries != null)
            {
                foreach (var entry in existingEntries)
                {
                    try
                    {
                        var existingSummary = JsonSerializer.Deserialize<CachedSummaryDto>(entry);
                        if (existingSummary?.Id == summary.Id)
                        {
                            await db.SortedSetRemoveAsync(timelineKey, entry);
                            break;
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip invalid JSON entries
                        continue;
                    }
                }
            }

            // Add the new or updated entry
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

    private IEnumerable<T> DeserializeDigests<T>(RedisValue[] digests)
    {
        return digests
            .Select(d => JsonSerializer.Deserialize<T>(d!))
            .Where(d => d != null)
            .Cast<T>();
    }
} 