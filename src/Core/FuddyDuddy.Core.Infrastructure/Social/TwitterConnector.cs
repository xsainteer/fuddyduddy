using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Language = FuddyDuddy.Core.Domain.Entities.Language;
using System.Text;
using FuddyDuddy.Core.Application.Constants;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnector : ITwitterConnector
{
    private readonly ILogger<TwitterConnector> _logger;
    private readonly TwitterOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Language _language;
    private const string TWITTER_API_V2_ENDPOINT = "https://api.x.com/2/tweets";
    private const string TWITTER_OAUTH_TOKEN_ENDPOINT = "https://api.x.com/2/oauth2/token";

    public TwitterConnector(
        IOptions<TwitterOptions> options,
        ILogger<TwitterConnector> logger,
        IHttpClientFactory httpClientFactory,
        Language language)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _language = language;
    }

    private async Task<string?> GetBearerTokenAsync(TwitterOptions.Credentials credentials, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
            
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, TWITTER_OAUTH_TOKEN_ENDPOINT)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get bearer token. Response: {ErrorContent}", error);
                return null;
            }

            var tokenResponse = await JsonSerializer.DeserializeAsync<JsonElement>(
                await response.Content.ReadAsStreamAsync(cancellationToken), 
                cancellationToken: cancellationToken);
            
            return tokenResponse.GetProperty("access_token").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bearer token");
            return null;
        }
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
            if (!_options.LanguageDict.TryGetValue(_language, out var credentials))
            {
                _logger.LogError("No Twitter credentials configured for language {Language}", _language);
                return false;
            }

            var bearerToken = await GetBearerTokenAsync(credentials, cancellationToken);
            if (bearerToken == null)
            {
                return false;
            }

            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
            var request = new HttpRequestMessage(HttpMethod.Post, TWITTER_API_V2_ENDPOINT)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { text = content }),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted tweet: {Content}", content);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to post tweet: {Content}. Response: {ErrorContent}", 
                content, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet: {Content}", content);
            return false;
        }
    }
} 