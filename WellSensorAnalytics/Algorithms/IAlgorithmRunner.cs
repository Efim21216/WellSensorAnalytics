using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics.Algorithms;

public interface IAlgorithmRunner
{
    Task RunAsync(Algorithm setting, CancellationToken ct);
}
