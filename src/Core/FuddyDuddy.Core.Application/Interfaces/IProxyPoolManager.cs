namespace FuddyDuddy.Core.Application.Interfaces;

public interface IProxyPoolManager
{
    string? GetNextProxy();
    void ReportFailure(string proxyAddress);
    void ReportSuccess(string proxyAddress);
}