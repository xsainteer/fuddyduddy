using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.Broker;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application;
using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Application.Configuration;
using Microsoft.Extensions.Options;

namespace FuddyDuddy.Api.HostedServices;

public class SimilarRequestListener : IHostedService
{
    private readonly ILogger<SimilarRequestListener> _logger;
    private readonly IBroker _broker;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<SimilaritySettings> _similaritySettings;
    public SimilarRequestListener(
        ILogger<SimilarRequestListener> logger,
        IBroker broker,
        IServiceProvider serviceProvider,
        IOptions<SimilaritySettings> similaritySettings)
    {
        _logger = logger;
        _broker = broker;
        _serviceProvider = serviceProvider;
        _similaritySettings = similaritySettings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_similaritySettings.Value.Enabled)
        {
            _logger.LogWarning("Similarity service is disabled");
            return Task.CompletedTask;
        }

        _broker.ConsumeAsync<SimilarRequest>(
            QueueNameConstants.Similar,
            async (message) => await HandleSimilarRequestAsync(message),
            _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private async Task HandleSimilarRequestAsync(SimilarRequest message)
    {
        if (message.NewsSummaryId == Guid.Empty)
        {
            _logger.LogError("News summary ID is empty");
            return;
        }

        var service = _serviceProvider.GetRequiredService<ISimilarityService>();
        await service.FindSimilarSummariesAsync(message.NewsSummaryId, _cancellationTokenSource.Token);
        _logger.LogInformation("Similar request for news summary {NewsSummaryId} processed", message.NewsSummaryId);
    }
}