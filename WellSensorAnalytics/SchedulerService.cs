using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WellSensorAnalytics.Algorithms;
using WellSensorAnalytics.Authentication;
using WellSensorAnalytics.Data;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics;

public sealed class SchedulerService(ILogger<SchedulerService> logger, IServiceScopeFactory serviceScopeFactory,
    IHttpClientFactory httpClientFactory, IAlgorithmRunner runner, IAuthService authService,
    IOptions<SchedulerOptions> options) : BackgroundService
{
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };
    private readonly HttpClient _apiClient = httpClientFactory.CreateClient("ApiClient");
    private readonly TimeSpan _syncInterval = options.Value.SyncInterval;
    private readonly Dictionary<int, ScheduledTask> _tasks = [];
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("SchedulerService starting");

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
        var scheduled = new ScheduledTask(algorithm, runner, serviceScopeFactory, logger);
        scheduled.Start();
        return scheduled;
    }

    private async Task TestDb(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<AnalyticsDbContext>()!;
                var algorithms = await db.Algorithms.ToListAsync(cancellationToken: stoppingToken);
                logger.LogInformation("Algorithms: {json}\n", JsonSerializer.Serialize(algorithms, options));
            }
            await Task.Delay(5_000, stoppingToken);
        }
    }
    private async Task TestAuth(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker starting up. Performing initial login...");
        bool loggedIn = await authService.LoginAsync();

        if (!loggedIn)
        {
            logger.LogCritical("Initial login failed. The service cannot proceed and will stop.");
            return;
        }
        else
        {
            logger.LogInformation("Authentication is successful!");
        }
        int channelId = 1;
        long from = 1756026994764;
        var response = await _apiClient.GetAsync(
            $"/api/v1/data-harvesters/channels/{channelId}?from={from}", stoppingToken);
        var body = await response.Content.ReadAsStringAsync(stoppingToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("OAuth request failed with status {StatusCode}. Error message: {Error}", response.StatusCode, body);
            return;
        }

        await using var networkStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var fileStream = File.Create($"dump_{channelId}_{from}.csv");


        await networkStream.CopyToAsync(fileStream, stoppingToken).ConfigureAwait(false);

        return;
    }
}
