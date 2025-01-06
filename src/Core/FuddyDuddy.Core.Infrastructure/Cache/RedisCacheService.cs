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
    private const string SUMMARIES_KEY = "latest:summaries";
    private const int MAX_SUMMARIES = 1000;

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
            
            await db.SortedSetAddAsync(
                SUMMARIES_KEY,
                JsonSerializer.Serialize(cachedSummary),
                score);

            // Trim to keep only latest 1000
            await db.SortedSetRemoveRangeByRankAsync(SUMMARIES_KEY, 0, -MAX_SUMMARIES - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding summary to cache");
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetLatestSummariesAsync<T>(int skip, int take, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Get total count for pagination
            var totalCount = await db.SortedSetLengthAsync(SUMMARIES_KEY);
            
            // If skip is beyond total count, return empty
            if (skip >= totalCount)
                return Enumerable.Empty<T>();

            var summaries = await db.SortedSetRangeByRankAsync(
                SUMMARIES_KEY,
                start: skip,
                stop: Math.Min(skip + take - 1, totalCount - 1),
                Order.Descending);

            if (summaries == null || !summaries.Any())
                return Enumerable.Empty<T>();

            return summaries
                .Select(s => JsonSerializer.Deserialize<T>(s.ToString()!))
                .Where(s => s != null)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest summaries from cache. Skip: {Skip}, Take: {Take}", skip, take);
            throw;
        }
    }

    public async Task<IEnumerable<T>?> GetSummariesAroundIdAsync<T>(
        string summaryId,
        int count,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var allSummaries = await db.SortedSetRangeByScoreAsync(SUMMARIES_KEY, order: Order.Descending);
            
            if (allSummaries == null || !allSummaries.Any())
                return null;

            var targetIndex = -1;
            var summariesList = new List<T>();

            // Find the target summary
            for (var i = 0; i < allSummaries.Length; i++)
            {
                var summary = JsonSerializer.Deserialize<T>(allSummaries[i]!.ToString()!);
                summariesList.Add(summary!);
                
                var id = summary!.GetType().GetProperty("Id")!.GetValue(summary)?.ToString();
                if (id == summaryId)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex == -1)
            {
                return null;
            }

            // Calculate the range to fetch
            var halfCount = count / 2;
            var start = Math.Max(0, targetIndex - halfCount);
            var end = Math.Min(allSummaries.Length - 1, targetIndex + halfCount);

            return allSummaries
                .Skip(start)
                .Take(end - start + 1)
                .Select(s => JsonSerializer.Deserialize<T>(s.ToString()!)!)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summaries around ID from cache. SummaryId: {SummaryId}", summaryId);
            throw;
        }
    }

    public async Task<T?> GetSummaryByIdAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        var key = $"summary:{id}";
        var value = await _redis.GetDatabase().StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task CacheSummaryDtoAsync<T>(string id, T summary, CancellationToken cancellationToken = default)
    {
        var key = $"summary:{id}";
        var value = JsonSerializer.Serialize(summary);
        await _redis.GetDatabase().StringSetAsync(key, value, TimeSpan.FromHours(24));
    }

    public async Task RebuildCacheAsync(IEnumerable<NewsSummary> summaries, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            // Clear existing cache
            await db.KeyDeleteAsync(SUMMARIES_KEY);
            
            // Add all summaries to cache
            foreach (var summary in summaries)
            {
                var score = summary.GeneratedAt.ToUnixTimeSeconds();
                var cachedSummary = CachedSummaryDto.FromNewsSummary(summary);
                
                await db.SortedSetAddAsync(
                    SUMMARIES_KEY,
                    JsonSerializer.Serialize(cachedSummary),
                    score);
            }
            
            // Trim to keep only latest 1000
            await db.SortedSetRemoveRangeByRankAsync(SUMMARIES_KEY, 0, -MAX_SUMMARIES - 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding cache");
            throw;
        }
    }
} 