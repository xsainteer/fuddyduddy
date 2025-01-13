using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnector : ITwitterConnector
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TwitterConnector> _logger;
    private readonly TwitterOptions _options;

    public TwitterConnector(
        IHttpClientFactory httpClientFactory,
        IOptions<TwitterOptions> options,
        ILogger<TwitterConnector> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
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
            using var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.TWITTER);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.LanguageDict[language].BearerToken);
            
            var payload = new { text = content };
            var json = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("tweets", requestContent, cancellationToken);
            
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
} 