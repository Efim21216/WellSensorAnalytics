using System;
using Microsoft.Extensions.Logging;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics.Algorithms;

public class AlgorithmRunnerStub(ILogger<AlgorithmRunnerStub> logger) : IAlgorithmRunner
{
    private readonly ILogger<AlgorithmRunnerStub> _logger = logger;

    public async Task RunAsync(Algorithm setting, CancellationToken ct)
    {
        _logger.LogInformation("Start algorithm {Name} (id={Id}) for well {WellId}. Timestamp: {Timestamp}", setting.Name, setting.Id, setting.WaterWellId, DateTime.Now);

        await Task.Delay(TimeSpan.FromSeconds(5), ct);

        _logger.LogInformation("Finished algorithm {Id}", setting.Id);
    }
}
