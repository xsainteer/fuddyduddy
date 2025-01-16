using FuddyDuddy.Core.Application.Services;
using FuddyDuddy.Core.Domain.Entities;
using Microsoft.Extensions.Options;
using FuddyDuddy.Api.Configuration;

namespace FuddyDuddy.Api.HostedServices;

public class SimpleTaskScheduler : IHostedService, IDisposable
{
    private readonly ILogger<SimpleTaskScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<TaskSchedulerSettings> _schedulerSettings;
    private Timer? _summaryPipelineTimer;
    private Timer? _digestPipelineTimer;
    private readonly SemaphoreSlim _pipelineLock = new(1, 1);
    private readonly SemaphoreSlim _digestLock = new(1, 1);
    public SimpleTaskScheduler(
        ILogger<SimpleTaskScheduler> logger,
        IServiceProvider serviceProvider,
        IOptions<TaskSchedulerSettings> schedulerSettings)
    {
        _logger = logger;
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

            _logger.LogInformation("Starting summary pipeline execution");

            // Step 1: Process news
            if (_schedulerSettings.Value.SummaryTask)
            {
                var newsProcessingService = _serviceProvider.GetRequiredService<INewsProcessingService>();
                await newsProcessingService.ProcessNewsSourcesAsync(cancellationToken);
                _logger.LogInformation("News processing completed");
            }

            // Step 2: Check for similar summaries
            if (_schedulerSettings.Value.SimilarityTask)
            {
                var similarityService = _serviceProvider.GetRequiredService<ISimilarityService>();
                await similarityService.CheckForSimilarSummariesAsync(cancellationToken);
                _logger.LogInformation("Similar summaries check completed");
            }

            // Step 3: Validate summaries
            if (_schedulerSettings.Value.ValidationTask)
            {
                var validationService = _serviceProvider.GetRequiredService<ISummaryValidationService>();
                await validationService.ValidateNewSummariesAsync(cancellationToken);
                _logger.LogInformation("Summary validation completed");
            }

            // Step 4: Translate to English
            if (_schedulerSettings.Value.TranslationTask)
            {
                var translationService = _serviceProvider.GetRequiredService<ISummaryTranslationService>();
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
            await _digestLock.WaitAsync(cancellationToken);

            if (!_schedulerSettings.Value.DigestTask)
            {
                _logger.LogInformation("Digest pipeline is disabled");
                return;
            }

            await GenerateDigestAsync(Language.RU, cancellationToken);
            await GenerateDigestAsync(Language.EN, cancellationToken);

            await GenerateTweetAsync(Language.RU, cancellationToken);
            await GenerateTweetAsync(Language.EN, cancellationToken);

            _logger.LogInformation("Digest pipeline execution completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing digest pipeline");
        }
        finally
        {
            _digestLock.Release();
        }
    }

    private async Task GenerateDigestAsync(Language language, CancellationToken cancellationToken)
    {
        var digestCookService = _serviceProvider.GetRequiredService<IDigestCookService>();

        _logger.LogInformation("Starting digest pipeline execution");

        // Generate Russian digest
        await digestCookService.GenerateDigestAsync(language, cancellationToken);
        _logger.LogInformation("{Language} digest generation completed", language);
    }

    private async Task GenerateTweetAsync(Language language, CancellationToken cancellationToken)
    {
        var digestCookService = _serviceProvider.GetRequiredService<IDigestCookService>();
        await digestCookService.GenerateTweetAsync(language, cancellationToken);
        _logger.LogInformation("{Language} tweet generation completed", language);
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