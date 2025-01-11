using FuddyDuddy.Core.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FuddyDuddy.Core.Application.Workers;

public abstract class BaseWorker : IHostedService, IDisposable
{
    private readonly IBroker _broker;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _stoppingCts;
    private Task? _executingTask;

    protected BaseWorker(IBroker broker, ILogger logger)
    {
        _broker = broker;
        _logger = logger;
        _stoppingCts = new CancellationTokenSource();
    }

    protected abstract string QueueName { get; }
    protected abstract Task ProcessMessageAsync<T>(T message) where T : class;

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{WorkerName} is starting...", GetType().Name);

        _executingTask = ExecuteAsync(_stoppingCts.Token);

        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }

        return Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _stoppingCts.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    protected virtual async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{WorkerName} is executing...", GetType().Name);

        try
        {
            await _broker.ConsumeAsync<object>(QueueName, async message =>
            {
                await ProcessMessageAsync(message);
            }, stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error occurred executing {WorkerName}", GetType().Name);
            throw;
        }
    }

    public virtual void Dispose()
    {
        _stoppingCts.Cancel();
        _stoppingCts.Dispose();
        GC.SuppressFinalize(this);
    }
} 