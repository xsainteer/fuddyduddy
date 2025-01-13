using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ITwitterConnector
{
    Task<bool> PostTweetAsync(Language language, string content, CancellationToken cancellationToken = default);
} 