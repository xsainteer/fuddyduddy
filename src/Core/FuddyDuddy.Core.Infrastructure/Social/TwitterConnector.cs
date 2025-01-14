using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Constants;
using Language = FuddyDuddy.Core.Domain.Entities.Language;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using FuddyDuddy.Core.Application.Models;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnector : ITwitterConnector
{
    private readonly ILogger<TwitterConnector> _logger;
    private readonly TwitterOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Language _language;
    private readonly ITwitterAuthService _authService;
    private readonly ICacheService _cache;

    public TwitterConnector(
        Language language,
        IOptions<TwitterOptions> options,
        ILogger<TwitterConnector> logger,
        IHttpClientFactory httpClientFactory,
        ITwitterAuthService authService,
        ICacheService cache)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _language = language;
        _authService = authService;
        _cache = cache;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var cachedToken = await _cache.GetTwitterTokenAsync(_language, cancellationToken);
        
        if (cachedToken?.AccessToken != null)
        {
            // Check if token is about to expire (within 5 minutes)
            if (cachedToken.ExpiresIn > 300 && !string.IsNullOrEmpty(cachedToken.RefreshToken))
            {
                try
                {
                    // Try to refresh the token
                    var newToken = await RefreshTokenAsync(cachedToken.RefreshToken, cancellationToken);
                    return newToken;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh Twitter token, will try to use existing token");
                    return cachedToken.AccessToken;
                }
            }
            
            return cachedToken.AccessToken;
        }

        // If no cached token, start new auth flow
        var authUrl = await _authService.GetAuthorizationUrlAsync(_language, cancellationToken);
        _logger.LogInformation("Please authorize the application at: {AuthUrl}", authUrl);
        throw new UnauthorizedAccessException($"Twitter authorization required. Please visit: {authUrl}");
    }

    private async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (!_options.LanguageDict.TryGetValue(_language, out var credentials))
        {
            throw new InvalidOperationException($"No Twitter credentials configured for language {_language}");
        }

        using var client = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = credentials.ClientId
        });

        var basicAuth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

        var response = await client.PostAsync("2/oauth2/token", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to refresh token: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TwitterTokenResponse>(
            cancellationToken: cancellationToken);

        if (tokenResponse?.AccessToken == null)
        {
            throw new InvalidOperationException("No access token in refresh response");
        }

        // Update cache with new token
        await _cache.SetTwitterTokenAsync(
            _language,
            new CachedTwitterTokenResponseDto
            {
                AccessToken = tokenResponse.AccessToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                RefreshToken = tokenResponse.RefreshToken ?? refreshToken, // Keep old refresh token if not provided
                Scope = tokenResponse.Scope
            },
            cancellationToken);

        return tokenResponse.AccessToken;
    }

    public async Task<bool> PostTweetAsync(string content, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Twitter posting is disabled");
            return false;
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.PostAsJsonAsync(
                "2/tweets",
                new { text = content },
                cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted tweet: {Content}", content);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // If token is expired or invalid, try to get a new one
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _cache.SetTwitterTokenAsync(_language, null!, cancellationToken); // Clear cached token
                _logger.LogWarning("Twitter token expired, please reauthorize");
                var authUrl = await _authService.GetAuthorizationUrlAsync(_language, cancellationToken);
                throw new UnauthorizedAccessException($"Twitter authorization required. Please visit: {authUrl}");
            }

            _logger.LogError("Failed to post tweet: {Content}. Response: {ErrorContent}", 
                content, errorContent);
            return false;
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Error posting tweet: {Content}", content);
            return false;
        }
    }

    private class TwitterTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
} 