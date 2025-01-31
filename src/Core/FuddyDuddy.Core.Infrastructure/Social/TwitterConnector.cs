using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Language = FuddyDuddy.Core.Domain.Entities.Language;
using System.Text;
using System.Security.Cryptography;
using FuddyDuddy.Core.Application.Constants;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnector : ITwitterConnector
{
    private readonly ILogger<TwitterConnector> _logger;
    private readonly TwitterOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Language _language;
    private const string TWITTER_API_ENDPOINT = "https://api.twitter.com/2/tweets";

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

    private string GenerateOAuthSignature(string httpMethod, string url, IDictionary<string, string> parameters, string requestBody, string consumerSecret, string tokenSecret)
    {
        // Include request body hash in parameters
        using var sha1 = SHA1.Create();
        var bodyHash = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(requestBody)));
        parameters.Add("oauth_body_hash", bodyHash);

        var parameterString = string.Join("&",
            parameters.OrderBy(p => p.Key)
                     .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        var signatureBaseString = $"{httpMethod}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(parameterString)}";
        var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret)}";

        using var hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
        var signatureBytes = hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(signatureBaseString));
        return Convert.ToBase64String(signatureBytes);
    }

    private string GenerateNonce()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "").Replace("/", "").Replace("=", "");
    }

    public async Task<bool> PostTweetAsync(string tweetText, CancellationToken cancellationToken = default)
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

            var nonce = GenerateNonce();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // Create request body first
            var requestBody = JsonSerializer.Serialize(new { text = tweetText });

            // OAuth parameters
            var parameters = new Dictionary<string, string>
            {
                { "oauth_consumer_key", credentials.ConsumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", credentials.AccessToken },
                { "oauth_version", "1.0" }
            };

            var signature = GenerateOAuthSignature("POST", TWITTER_API_ENDPOINT, parameters, 
                requestBody, credentials.ConsumerSecret, credentials.AccessTokenSecret);

            // Calculate body hash for header
            using var sha1 = SHA1.Create();
            var bodyHash = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(requestBody)));

            // Format OAuth header without spaces after commas
            var authHeader = "OAuth " +
                string.Join(",", new[]
                {
                    $"oauth_consumer_key=\"{Uri.EscapeDataString(credentials.ConsumerKey)}\"",
                    $"oauth_nonce=\"{Uri.EscapeDataString(nonce)}\"",
                    $"oauth_signature=\"{Uri.EscapeDataString(signature)}\"",
                    $"oauth_signature_method=\"HMAC-SHA1\"",
                    $"oauth_timestamp=\"{Uri.EscapeDataString(timestamp)}\"",
                    $"oauth_token=\"{Uri.EscapeDataString(credentials.AccessToken)}\"",
                    $"oauth_version=\"1.0\"",
                    $"oauth_body_hash=\"{Uri.EscapeDataString(bodyHash)}\""
                });

            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            var request = new HttpRequestMessage(HttpMethod.Post, TWITTER_API_ENDPOINT)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", authHeader);
            request.Headers.Add("User-Agent", "FuddyDuddy/1.0");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted tweet: {Content}", tweetText);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to post tweet: {Content}. Response: {ErrorContent}", 
                tweetText, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet: {Content}", tweetText);
            return false;
        }
    }
} 