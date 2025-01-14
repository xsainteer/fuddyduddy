using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using FuddyDuddy.Core.Application.Configuration;
using FuddyDuddy.Core.Application.Constants;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterAuthService : ITwitterAuthService
{
    private readonly ICacheService _cache;
    private readonly TwitterOptions _options;
    private readonly ProcessingOptions _processingOptions;
    private readonly ILogger<TwitterAuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    private const string TWITTER_AUTH_ENDPOINT = "https://x.com/i/oauth2/authorize";
    private const string TWITTER_TOKEN_ENDPOINT = "https://api.x.com/2/oauth2/token";
    private const string TWITTER_SCOPES = "tweet.read tweet.write users.read offline.access";

    public TwitterAuthService(
        ICacheService cache,
        IOptions<TwitterOptions> options,
        IOptions<ProcessingOptions> processingOptions,
        ILogger<TwitterAuthService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _cache = cache;
        _options = options.Value;
        _processingOptions = processingOptions.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetAuthorizationUrlAsync(Language language, CancellationToken cancellationToken = default)
    {
        if (!_options.LanguageDict.TryGetValue(language, out var credentials))
        {
            throw new InvalidOperationException($"No Twitter credentials configured for language {language}");
        }

        var state = Guid.NewGuid().ToString("N");
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        // Store PKCE and state values in cache
        await _cache.SetTwitterAuthStateAsync(
            state,
            new CachedTwitterAuthStateDto
            {
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                Language = language
            },
            cancellationToken);

        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = credentials.ClientId,
            ["redirect_uri"] = BuildRedirectUri(),
            ["scope"] = TWITTER_SCOPES,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", queryParams.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{TWITTER_AUTH_ENDPOINT}?{queryString}";
    }

    public async Task<string> HandleCallbackAsync(string code, string state, CancellationToken cancellationToken = default)
    {
        var authState = await _cache.GetTwitterAuthStateAsync(state, cancellationToken);
        
        if (authState == null)
        {
            throw new InvalidOperationException("Invalid or expired state parameter");
        }

        // Remove the state from cache to prevent replay attacks
        await _cache.RemoveTwitterAuthStateAsync(state, cancellationToken);

        if (!_options.LanguageDict.TryGetValue(authState.Language, out var credentials))
        {
            throw new InvalidOperationException($"No Twitter credentials configured for language {authState.Language}");
        }

        using var client = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["client_id"] = credentials.ClientId,
            ["redirect_uri"] = BuildRedirectUri(),
            ["code_verifier"] = authState.CodeVerifier
        });

        var basicAuth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

        var response = await client.PostAsync(TWITTER_TOKEN_ENDPOINT, content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to obtain access token: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TwitterTokenResponse>(
            cancellationToken: cancellationToken);

        if (tokenResponse?.AccessToken == null)
        {
            throw new InvalidOperationException("No access token in response");
        }

        // Store the access token in cache
        await _cache.SetTwitterTokenAsync(
            authState.Language,
            new CachedTwitterTokenResponseDto
            {
                AccessToken = tokenResponse.AccessToken,
                TokenType = tokenResponse.TokenType,
                ExpiresIn = tokenResponse.ExpiresIn,
                RefreshToken = tokenResponse.RefreshToken,
                Scope = tokenResponse.Scope
            },
            cancellationToken);

        return tokenResponse.AccessToken;
    }

    private string BuildRedirectUri() => $"https://{_processingOptions.Domain}/api/callback/twitter";

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
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
