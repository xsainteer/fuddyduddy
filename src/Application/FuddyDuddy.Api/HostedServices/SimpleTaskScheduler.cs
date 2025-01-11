using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Options;
using FuddyDuddy.Api.Configuration;

namespace FuddyDuddy.Api.HostedServices;

public class SimpleTaskScheduler : IHostedService, IDisposable
{
    private readonly ILogger<SimpleTaskScheduler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<TaskSchedulerSettings> _schedulerSettings;
    private Timer? _summaryPipelineTimer;
    private Timer? _digestPipelineTimer;
    private readonly SemaphoreSlim _pipelineLock = new(1, 1);

    public SimpleTaskScheduler(
        ILogger<SimpleTaskScheduler> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IOptions<TaskSchedulerSettings> schedulerSettings)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _schedulerSettings = schedulerSettings;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_schedulerSettings.Value.Enabled)
        {
            _logger.LogWarning("Task Scheduler Service is disabled.");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Task Scheduler Service is starting.");

        // Get intervals from configuration
        var summaryInterval = _schedulerSettings.Value.SummaryPipelineInterval;
        var digestInterval = _schedulerSettings.Value.DigestPipelineInterval;

        // Start summary pipeline timer
        _summaryPipelineTimer = new Timer(
            async _ => await ExecuteSummaryPipelineAsync(cancellationToken),
            null,
            TimeSpan.Zero, // Start immediately
            summaryInterval
        );

        // Start digest pipeline timer
        _digestPipelineTimer = new Timer(
            async _ => await ExecuteDigestPipelineAsync(cancellationToken),
            null,
            TimeSpan.Zero, // Start immediately
            digestInterval
        );

        return Task.CompletedTask;
    }

    private async Task ExecuteSummaryPipelineAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _pipelineLock.WaitAsync(cancellationToken);

            using var scope = _serviceProvider.CreateScope();
            var newsProcessingService = scope.ServiceProvider.GetRequiredService<INewsProcessingService>();
            var validationService = scope.ServiceProvider.GetRequiredService<ISummaryValidationService>();
            var translationService = scope.ServiceProvider.GetRequiredService<ISummaryTranslationService>();

            _logger.LogInformation("Starting summary pipeline execution");

            // Step 1: Process news
            if (_schedulerSettings.Value.SummaryTask)
            {
                await newsProcessingService.ProcessNewsSourcesAsync(cancellationToken);
                _logger.LogInformation("News processing completed");
            }

            // Step 2: Validate summaries
            if (_schedulerSettings.Value.ValidationTask)
            {
                await validationService.ValidateNewSummariesAsync(cancellationToken);
                _logger.LogInformation("Summary validation completed");
            }

            // Step 3: Translate to English
            if (_schedulerSettings.Value.TranslationTask)
            {
                await translationService.TranslatePendingAsync(Language.EN, cancellationToken);
                _logger.LogInformation("Summary translation completed");
            }

            _logger.LogInformation("Summary pipeline execution completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing summary pipeline");
        }
        finally
        {
            _pipelineLock.Release();
        }
    }

    private async Task ExecuteDigestPipelineAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _pipelineLock.WaitAsync(cancellationToken);

            if (!_schedulerSettings.Value.DigestTask)
            {
                _logger.LogInformation("Digest pipeline is disabled");
                return;
            }

            var (startHour, endHour) = _schedulerSettings.Value.DigestTaskInactivityHoursRangeTuple;
            if (DateTime.Now.Hour >= startHour || DateTime.Now.Hour < endHour)
            {
                _logger.LogInformation("Digest pipeline is disabled due to current time");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var digestCookService = scope.ServiceProvider.GetRequiredService<IDigestCookService>();

            _logger.LogInformation("Starting digest pipeline execution");

            // Generate Russian digest
            await digestCookService.GenerateDigestAsync(Language.RU, cancellationToken);
            _logger.LogInformation("Russian digest generation completed");

            // Generate English digest
            await digestCookService.GenerateDigestAsync(Language.EN, cancellationToken);
            _logger.LogInformation("English digest generation completed");

            _logger.LogInformation("Digest pipeline execution completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing digest pipeline");
        }
        finally
        {
            _pipelineLock.Release();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Scheduler Service is stopping.");

        _summaryPipelineTimer?.Change(Timeout.Infinite, 0);
        _digestPipelineTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _summaryPipelineTimer?.Dispose();
        _digestPipelineTimer?.Dispose();
    }
}