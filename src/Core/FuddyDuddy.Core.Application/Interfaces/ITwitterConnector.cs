using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ITwitterConnector
{
    Task<bool> PostTweetAsync(string content, CancellationToken cancellationToken = default);
} 