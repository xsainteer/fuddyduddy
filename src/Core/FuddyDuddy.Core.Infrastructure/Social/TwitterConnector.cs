using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Domain.Entities;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnector : ITwitterConnector
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TwitterConnector> _logger;
    private readonly TwitterOptions _options;
    private readonly ICacheService _cacheService;

    public TwitterConnector(
        IHttpClientFactory httpClientFactory,
        IOptions<TwitterOptions> options,
        ICacheService cacheService,
        ILogger<TwitterConnector> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> PostTweetAsync(Language language, string content, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Twitter posting is disabled");
            return false;
        }

        try
        {
            var accessToken = await GetAccessTokenAsync(language, cancellationToken);
            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await httpClient.PostAsync("tweets", 
                new StringContent(JsonSerializer.Serialize(new { text = content }), 
                Encoding.UTF8, 
                "application/json"), 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to post tweet. Status: {Status}, Error: {Error}", 
                    response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Successfully posted tweet: {Content}", content);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet: {Content}", content);
            return false;
        }
    }

    private async Task<string> GetAccessTokenAsync(Language language, CancellationToken cancellationToken)
    {
        var cachedToken = await _cacheService.GetTwitterTokenAsync(language, cancellationToken);
        
        if (cachedToken != null)
            return cachedToken;

        using var client = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
        
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{_options.LanguageDict[language].ApiKey}:{_options.LanguageDict[language].SecretKey}"));
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "tweet.read tweet.write users.read")
        });

        var response = await client.PostAsync("oauth2/token", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to obtain Twitter access token: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<TwitterAuthResponse>(cancellationToken: cancellationToken);
        
        if (result?.AccessToken == null)
            throw new InvalidOperationException("Failed to obtain Twitter access token");

        await _cacheService.SetTwitterTokenAsync(language, result.AccessToken, 
            TimeSpan.FromSeconds(result.ExpiresIn - 60),
            cancellationToken);

        return result.AccessToken;
    }

    private record TwitterAuthResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    )
    {
        public bool IsValid => !string.IsNullOrEmpty(AccessToken);
    }
} 