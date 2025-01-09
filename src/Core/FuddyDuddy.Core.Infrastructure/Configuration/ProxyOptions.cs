namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class ProxyOptions
{
    public const string SectionName = "Proxy";
    
    public List<string> Proxies { get; set; } = new();
    public int MaxFailures { get; set; } = 3;
    public int BanTimeoutMinutes { get; set; } = 30;
    public bool UseProxies { get; set; } = true;
    public string? DefaultProxy { get; set; }
} 