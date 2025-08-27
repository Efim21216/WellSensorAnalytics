using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WellSensorAnalytics.Algorithms;
using WellSensorAnalytics.Data;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics;

public class ScheduledTask(
    Algorithm setting,
    IAlgorithmRunner runner,
    IServiceScopeFactory scopeFactory,
    ILogger logger)
{
    public Algorithm LastKnownSetting { get; private set; } = setting;
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _runningLock = new(1, 1);

    public Task Completion { get; private set; } = Task.CompletedTask;

    public void Start()
    {
        Completion = Task.Run(() => LoopAsync(_cts.Token));
    }

    public void Cancel()
    {
        try { _cts.Cancel(); } catch { /*ignore*/ }
    }
    private async Task LoopAsync(CancellationToken ct)
    {
        logger.LogDebug("ScheduledTask loop started for {Id}", LastKnownSetting.Id);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                await WaitScheduledTime(ct);

                // В нормальном сценарии никогда не зайдёт внутрь!
                if (!await _runningLock.WaitAsync(0, ct))
                {
                    logger.LogError("DANGER! Previous run of {Id} still executing!", LastKnownSetting.Id);
                    // skip this tick; wait until next interval
                    await Task.Delay(LastKnownSetting.ScheduleInterval, ct);
                    continue;
                }

                try
                {
                    await RunAlgorithm(ct);
                }
                finally
                {
                    _runningLock.Release();
                }
            }
        }
        catch (OperationCanceledException) { /* normal shutdown */ }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in ScheduledTask {Id}", LastKnownSetting.Id);
        }
        finally
        {
            logger.LogDebug("ScheduledTask loop ended for {Id}", LastKnownSetting.Id);
        }
    }
    private async Task RunAlgorithm(CancellationToken ct)
    {
        logger.LogDebug("Executing algorithm {Id}", LastKnownSetting.Id);

        using var scope = scopeFactory.CreateScope();
        await runner.RunAsync(LastKnownSetting, ct);

        var repository = scope.ServiceProvider.GetService<IAlgorithmRepository>()!;
        await repository.UpdateLastRunAsync(LastKnownSetting.Id, DateTimeOffset.UtcNow, ct);

        var refreshed = await repository.GetAlgorithmSettingAsync(LastKnownSetting.Id, ct);
        if (refreshed != null)
        {
            LastKnownSetting = refreshed;
        }
    }
    private async Task WaitScheduledTime(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        TimeSpan delay;

        if (LastKnownSetting.LastRun.HasValue)
        {
            var next = LastKnownSetting.LastRun.Value.Add(LastKnownSetting.ScheduleInterval);
            delay = next > now ? next - now : TimeSpan.Zero;
        }
        else
        {
            delay = TimeSpan.Zero;
        }

        if (delay > TimeSpan.Zero)
        {
            logger.LogDebug("Task {Id} sleeping for {Delay}", LastKnownSetting.Id, delay);
            await Task.Delay(delay, ct);
        }
    }
}
