using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ITwitterConnector
{
    Task<long?> PostTweetAsync(string content);
} 