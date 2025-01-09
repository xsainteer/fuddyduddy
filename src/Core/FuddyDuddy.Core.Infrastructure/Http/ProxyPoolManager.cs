using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Infrastructure.Configuration;
using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FuddyDuddy.Core.Infrastructure.Http;

public class ProxyPoolManager : IProxyPoolManager
{
    private readonly ConcurrentQueue<ProxyInfo> _proxyPool = new();
    private readonly ConcurrentDictionary<string, DateTime> _bannedProxies = new();
    private readonly ConcurrentDictionary<string, int> _failureCount = new();
    private readonly Random _random = new();
    private readonly ProxyOptions _options;
    private readonly ILogger<ProxyPoolManager> _logger;
    public ProxyPoolManager(IOptions<ProxyOptions> options, ILogger<ProxyPoolManager> logger)
    {
        _options = options.Value;
        _logger = logger;
        InitializeProxyPool();
    }

    private void InitializeProxyPool()
    {
        foreach (var proxy in _options.Proxies)
        {
            _proxyPool.Enqueue(new ProxyInfo(proxy));
        }
    }

    public string? GetNextProxy()
    {
        // Clean up old banned proxies
        var now = DateTime.UtcNow;
        var bannedToRemove = _bannedProxies
            .Where(x => (now - x.Value).TotalMinutes > _options.BanTimeoutMinutes)
            .Select(x => x.Key)
            .ToList();

        foreach (var proxy in bannedToRemove)
        {
            _bannedProxies.TryRemove(proxy, out _);
            _failureCount.TryRemove(proxy, out _);
        }

        // Try to get next available proxy
        while (_proxyPool.TryDequeue(out var proxy))
        {
            if (!_bannedProxies.ContainsKey(proxy.Address))
            {
                _proxyPool.Enqueue(proxy); // Put it back in the pool
                return proxy.Address;
            }
        }

        _logger.LogError("No available proxies");
        return null;
    }

    public void ReportFailure(string proxyAddress)
    {
        var failures = _failureCount.AddOrUpdate(
            proxyAddress,
            1,
            (_, count) => count + 1
        );

        if (failures >= _options.MaxFailures)
        {
            _bannedProxies.TryAdd(proxyAddress, DateTime.UtcNow);
        }
    }

    public void ReportSuccess(string proxyAddress)
    {
        _failureCount.TryRemove(proxyAddress, out _);
    }
}

public record ProxyInfo(string Address)
{
    public string Address { get; } = Address;
    public int FailureCount { get; set; }
    public DateTime? LastUsed { get; set; }
} 