using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface ITwitterAuthService
{
    Task<string> GetAuthorizationUrlAsync(Language language, CancellationToken cancellationToken = default);
    Task<string> HandleCallbackAsync(string code, string state, CancellationToken cancellationToken = default);
}