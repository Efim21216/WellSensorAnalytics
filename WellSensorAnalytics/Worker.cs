using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WellSensorAnalytics.Authentication;
using WellSensorAnalytics.Data;

namespace WellSensorAnalytics;

public sealed class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory,
    IHttpClientFactory httpClientFactory, IAuthService authService, IHostApplicationLifetime lifetime) : BackgroundService
{
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };
    private readonly HttpClient _apiClient = httpClientFactory.CreateClient("ApiClient");
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
        var response = await _apiClient.GetAsync("/api/v1/wells/data-harvesters/channels/1/values?start=1755857779000", stoppingToken);
        var body = await response.Content.ReadAsStringAsync(stoppingToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("OAuth request failed with status {StatusCode}. Error message: {Error}", response.StatusCode, body);
        }
        else
        {
            logger.LogInformation("Response: {body}", body);
        }

        lifetime.StopApplication();

        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     using (var scope = serviceScopeFactory.CreateScope())
        //     {
        //         var db = scope.ServiceProvider.GetService<AnalyticsDbContext>()!;
        //         var algorithms = await db.Algorithms.ToListAsync(cancellationToken: stoppingToken);
        //         logger.LogInformation("Algorithms: {json}\n", JsonSerializer.Serialize(algorithms, options));
        //     }
        //     await Task.Delay(5_000, stoppingToken);
        // }
    }
}
