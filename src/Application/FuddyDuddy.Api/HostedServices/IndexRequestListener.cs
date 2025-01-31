using FuddyDuddy.Core.Application.Interfaces;
using FuddyDuddy.Core.Application.Models.Broker;
using FuddyDuddy.Core.Application.Constants;
using FuddyDuddy.Core.Application;
using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Application.Configuration;
using Microsoft.Extensions.Options;

namespace FuddyDuddy.Api.HostedServices;

public class IndexRequestListener : IHostedService
{
    private readonly ILogger<IndexRequestListener> _logger;
    private readonly IBroker _broker;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<SearchSettings> _searchSettings;

    public IndexRequestListener(
        ILogger<IndexRequestListener> logger,
        IBroker broker,
        IServiceProvider serviceProvider,
        IOptions<SearchSettings> searchSettings)
    {
        _logger = logger;
        _broker = broker;
        _serviceProvider = serviceProvider;
        _searchSettings = searchSettings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_searchSettings.Value.Enabled)
        {
            _logger.LogWarning("Search service is disabled");
            return Task.CompletedTask;
        }

        _broker.ConsumeAsync<IndexRequest>(
            QueueNameConstants.Index,
            async (message) => await HandleIndexRequestAsync(message),
            _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private async Task HandleIndexRequestAsync(IndexRequest message)
    {
        if (message.NewsSummaryId == Guid.Empty)
        {
            _logger.LogError("News summary ID is empty");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var service = services.GetRequiredService<IVectorSearchService>();
        if (message.Type == IndexRequestType.Add)
        {
            await service.IndexSummaryAsync(message.NewsSummaryId, _cancellationTokenSource.Token);
        }
        else
        {
            await service.DeleteSummaryAsync(message.NewsSummaryId, _cancellationTokenSource.Token);
        }
        _logger.LogInformation("Index request for news summary {NewsSummaryId} processed", message.NewsSummaryId);
    }
}