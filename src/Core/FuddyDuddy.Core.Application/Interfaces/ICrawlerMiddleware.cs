namespace FuddyDuddy.Core.Application.Interfaces;

public interface ICrawlerMiddleware
{
    Task<HttpRequestMessage> PrepareRequestAsync(HttpRequestMessage request, string domain);
} 