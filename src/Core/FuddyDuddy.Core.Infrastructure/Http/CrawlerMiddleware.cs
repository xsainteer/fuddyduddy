using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Infrastructure.Configuration;
using FuddyDuddy.Core.Application.Interfaces;

namespace FuddyDuddy.Core.Infrastructure.Http;

public class CrawlerMiddleware : ICrawlerMiddleware
{
    private static readonly string[] CommonUserAgents = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/120.0.6099.119 Mobile/15E148 Safari/604.1",
        "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"
    };

    private readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();
    private readonly Random _random = new();
    private readonly CrawlerOptions _options;
    private int _currentUserAgentIndex;

    public CrawlerMiddleware(IOptions<CrawlerOptions> options)
    {
        _options = options.Value;
    }

    public async Task<HttpRequestMessage> PrepareRequestAsync(HttpRequestMessage request, string domain)
    {
        // Respect rate limiting
        if (_options.UseRandomDelay)
            await EnforceRateLimitingAsync(domain);

        // Rotate User-Agent
        var userAgent = GetNextUserAgent();
        request.Headers.UserAgent.ParseAdd(userAgent);

        // Add common browser headers
        request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "none");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");

        return request;
    }

    private string GetNextUserAgent()
    {
        // Round-robin through user agents
        var index = Interlocked.Increment(ref _currentUserAgentIndex) % CommonUserAgents.Length;
        return CommonUserAgents[index];
    }

    private async Task EnforceRateLimitingAsync(string domain)
    {
        var lastRequestTime = _lastRequestTimes.GetOrAdd(domain, DateTime.UtcNow);
        var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;

        // Random delay between 2-5 seconds
        var minDelay = TimeSpan.FromSeconds(2);
        var randomDelay = TimeSpan.FromMilliseconds(_random.Next(_options.MinDelayMilliseconds, _options.MaxDelayMilliseconds));
        var requiredDelay = _options.MinimumRequestInterval ?? minDelay;

        var effectiveDelay = TimeSpan.FromMilliseconds(Math.Max(
            randomDelay.TotalMilliseconds,
            (requiredDelay - timeSinceLastRequest).TotalMilliseconds
        ));

        if (effectiveDelay > TimeSpan.Zero)
        {
            await Task.Delay(effectiveDelay);
        }

        _lastRequestTimes[domain] = DateTime.UtcNow;
    }
} 