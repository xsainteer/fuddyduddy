using Microsoft.Extensions.Logging;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Infrastructure.RateLimit
{
    internal interface IRateLimiter
    {
        /// <summary>
        /// Tries to acquire token from bucket. Bucket assumes all values are second-based
        /// </summary>
        /// <param name="bucketKey"></param>
        /// <param name="ratePerSecond">Rate which the token should respect</param>
        /// <param name="maxTimeSec">Maximum time to wait (max delay)</param>
        /// <returns>Time to wait before token is allowed to execute. IMPORTANT: process that gets wait time greater than 0 should respect it in order for bucket to work properly.</returns>
        Task<double?> LeakyBucketGetTokenAsync(string bucketKey, int ratePerSecond, int maxTimeSec);

        Task<double?> GetMinuteTokenAsync(string bucketKey, int requestsPerMinute, int maxWaitTimeSec);
    }

    internal class RateLimiter: IRateLimiter
    {
        private readonly ILogger<RateLimiter> _logger;
        private readonly ICacheService _cacheService;

        // Modified script to handle per-minute rate limiting
        private readonly string _minuteRateLimitScript = @"
            local key = KEYS[1]
            local now = tonumber(ARGV[1])
            local rpm = tonumber(ARGV[2])      -- requests per minute
            local window = 60                  -- window size in seconds (1 minute)
            local maxWaitTime = tonumber(ARGV[3])

            -- Clean up old entries (older than window)
            local windowStart = now - window
            redis.call('ZREMRANGEBYSCORE', key, '-inf', windowStart)

            -- Count requests in current window
            local requestsInWindow = redis.call('ZCOUNT', key, windowStart, '+inf')

            -- If we haven't exceeded the rate limit
            if requestsInWindow < rpm then
                redis.call('ZADD', key, now, now)
                return '0'  -- no need to wait
            end

            -- Get the oldest timestamp in our window
            local oldestRequest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')[2]
            if oldestRequest then
                local nextAvailableSlot = tonumber(oldestRequest) + window
                local waitTime = nextAvailableSlot - now

                -- If wait time is acceptable, return it
                if waitTime <= maxWaitTime then
                    redis.call('ZADD', key, now + waitTime, now + waitTime)
                    return tostring(waitTime)
                end
            end

            return tostring(-1)  -- signal that we should reject the request
        ";

        public RateLimiter(ILogger<RateLimiter> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<double?> GetMinuteTokenAsync(string bucketKey, int requestsPerMinute, int maxWaitTimeSec)
        {
            try
            {
                _logger.LogInformation("Getting new token for minute-based rate limit using key {bucketKey} and limit {requestsPerMinute}/min", bucketKey, requestsPerMinute);
                var unixSeconds = (double)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var keys = new[] { bucketKey };
                var values = new[] { unixSeconds.ToString(), requestsPerMinute.ToString(), maxWaitTimeSec.ToString() };
                var result = await _cacheService.ExecuteLuaAsync(_minuteRateLimitScript, keys, values);
                
                if (result == null)
                {
                    _logger.LogError("Error while executing lua script for rate limiter: got null result");
                    return null;
                }

                if (!double.TryParse(result, out var timeToWait))
                {
                    _logger.LogError("Error while executing lua script for rate limiter: non-convertible result {result}", result);
                    return null;
                }

                // -1 indicates we should reject the request
                if (timeToWait < 0)
                {
                    _logger.LogWarning("Rate limit exceeded for {bucketKey}, max wait time would be exceeded", bucketKey);
                    return null;
                }

                return timeToWait;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while getting rate limit token");
                return null;
            }
        }

        public async Task<double?> LeakyBucketGetTokenAsync(string bucketKey, int ratePerSecond, int maxTimeSec)
        {
            var rpm = ratePerSecond * 60;
            return await GetMinuteTokenAsync(bucketKey, rpm, maxTimeSec);
        }
    }
}
