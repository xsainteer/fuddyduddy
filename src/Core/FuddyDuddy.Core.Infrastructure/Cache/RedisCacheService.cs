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

            var summaries = await db.SortedSetRangeByScoreAsync(
                SUMMARIES_KEY,
                order: Order.Descending,
                skip: skip,
                take: take);

            if (summaries == null || !summaries.Any())
                return Enumerable.Empty<T>();

            return summaries
                .Select(s => JsonSerializer.Deserialize<T>(s.ToString()!))
                .Where(s => s != null)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest summaries from cache");
            throw;
        }
    }
} 