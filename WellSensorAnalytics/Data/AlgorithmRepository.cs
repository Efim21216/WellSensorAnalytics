using System;
using Microsoft.EntityFrameworkCore;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics.Data;

public class AlgorithmRepository(AnalyticsDbContext db) : IAlgorithmRepository
{
    public async Task<Algorithm?> GetAlgorithmSettingAsync(int id, CancellationToken ct)
    {
        return await db.Algorithms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Algorithm>> GetEnabledAlgorithmSettingsAsync(CancellationToken ct)
    {
        return await db.Algorithms
            .Where(x => x.Enabled)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task UpdateLastRunAsync(int id, DateTimeOffset lastRun, CancellationToken ct)
    {
        var entity = await db.Algorithms.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return;
        entity.LastRun = lastRun;
        await db.SaveChangesAsync(ct);
    }
}
