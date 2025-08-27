using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics.Data;

public interface IAlgorithmRepository
{
    Task<IReadOnlyList<Algorithm>> GetEnabledAlgorithmSettingsAsync(CancellationToken ct);
    Task<Algorithm?> GetAlgorithmSettingAsync(int id, CancellationToken ct);

    Task UpdateLastRunAsync(int id, DateTimeOffset lastRun, CancellationToken ct);
}
