using Tweetinvi;
using Tweetinvi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Infrastructure.Configuration;
using Language = FuddyDuddy.Core.Domain.Entities.Language;

namespace FuddyDuddy.Core.Infrastructure.Social;

internal class TwitterConnector : ITwitterConnector
{
    private readonly ILogger<TwitterConnector> _logger;
    private readonly TwitterOptions _options;
    private readonly ITwitterClient _client;

    public TwitterConnector(
        IOptions<TwitterOptions> options,
        ILogger<TwitterConnector> logger,
        Language language)
    {
        _options = options.Value;
        _logger = logger;
        
        // Initialize clients for each language
        var creds = _options.LanguageDict[language];
        var twitterCredentials = new TwitterCredentials(
            creds.ConsumerKey,
            creds.ConsumerSecret,
            creds.AccessToken,
            creds.AccessTokenSecret
        );
        _client = new TwitterClient(twitterCredentials);
    }

    public async Task<long?> PostTweetAsync(string content)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Twitter posting is disabled");
            return null;
        }

        try
        {
            var tweet = await _client.Tweets.PublishTweetAsync(content);
            
            if (tweet != null)
            {
                _logger.LogInformation("Successfully posted tweet: {Content} with id {TweetId}", content, tweet.Id);
                return tweet.Id;
            }
            
            _logger.LogError("Failed to post tweet: {Content}", content);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting tweet: {Content}", content);
            return null;
        }
    }
} 