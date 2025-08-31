using System.Security.Authentication;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WellSensorAnalytics.Authentication;
using WellSensorAnalytics.Data;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics;

public sealed class SchedulerService(ILogger<SchedulerService> logger, IServiceScopeFactory serviceScopeFactory,
    IAuthService authService, IOptions<SchedulerOptions> options) : BackgroundService
{
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };
    private readonly TimeSpan _syncInterval = options.Value.SyncInterval;
    private readonly Dictionary<int, ScheduledTask> _tasks = [];
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SchedulerService starting up. Performing initial login...");
        bool loggedIn = await authService.LoginAsync();
        if (!loggedIn)
        {
            logger.LogCritical("Initial login failed. The service cannot proceed and will stop");
            throw new AuthenticationException("Initial login failed");
        }

        await RefreshSchedulesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
                await RefreshSchedulesAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while refreshing schedules");
            }
        }

        logger.LogDebug("SchedulerService stopping, cancelling tasks");
        foreach (var t in _tasks.Values)
        {
            t.Cancel();
        }

        await WaitTasks();
    }
    private async Task WaitTasks()
    {
        var wait = Task.WhenAll(_tasks.Values.Select(t => t.Completion));
        try { await wait.WaitAsync(TimeSpan.FromSeconds(30)); }
        catch { logger.LogWarning("Not all scheduled tasks finished in time"); }
    }
    private async Task RefreshSchedulesAsync(CancellationToken ct)
    {
        IReadOnlyList<Algorithm> algorithms;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var repository = scope.ServiceProvider.GetService<IAlgorithmRepository>()!;
            algorithms = await repository.GetEnabledAlgorithmSettingsAsync(ct);
        }

        var desiredIds = algorithms.Select(s => s.Id).ToHashSet();

        RemoveOutdatedTasks(desiredIds);

        AddOrUpdateTasks(algorithms);

    }
    private void RemoveOutdatedTasks(HashSet<int> desiredIds)
    {
        foreach (var existingId in _tasks.Keys)
        {
            if (!desiredIds.Contains(existingId))
            {
                if (_tasks.Remove(existingId, out var removed))
                {
                    logger.LogDebug("Stopping task for algorithm {Id} because it's removed/disabled", existingId);
                    removed.Cancel();
                }
            }
        }
    }
    private void AddOrUpdateTasks(IReadOnlyList<Algorithm> algorithms)
    {
        foreach (var algorithm in algorithms)
        {
            if (_tasks.TryGetValue(algorithm.Id, out var existingTask))
            {
                // Если задача уже существует, проверяем, изменились ли настройки. Они могут измениться, 
                // если кто-то другой будет обращаться к базе данных
                if (existingTask.LastKnownSetting.LastModified != algorithm.LastModified
                    || existingTask.LastKnownSetting.ScheduleInterval != algorithm.ScheduleInterval)
                {
                    logger.LogInformation("Settings changed for {Id}, restarting task", algorithm.Id);
                    existingTask.Cancel();
                    _tasks[algorithm.Id] = CreateAndStartScheduledTask(algorithm);
                }
            }
            else
            {
                // Если задача не существует, создаем и добавляем ее
                _tasks[algorithm.Id] = CreateAndStartScheduledTask(algorithm);
            }

        }
    }

    private ScheduledTask CreateAndStartScheduledTask(Algorithm algorithm)
    {
        var scheduled = new ScheduledTask(algorithm, serviceScopeFactory, logger);
        scheduled.Start();
        return scheduled;
    }
}
