using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WellSensorAnalytics.Data;

namespace WellSensorAnalytics;

public sealed class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly JsonSerializerOptions options = new(){ WriteIndented = true};
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
}
