using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WellSensorAnalytics.Data;
using WellSensorAnalytics.Models.Entities;
using WellSensorAnalytics.Models.Entities.Jsons;
using WellSensorAnalytics.Models.Exceptions;

namespace WellSensorAnalytics.Algorithms;

public class AlgorithmRunner(ILogger<SchedulerService> logger, AnalyticsDbContext db,
    IHttpClientFactory httpClientFactory) : IAlgorithmRunner
{
    private readonly HttpClient _apiClient = httpClientFactory.CreateClient("ApiClient");
    public async Task RunAsync(Algorithm algorithm, CancellationToken ct)
    {
        logger.LogInformation("Start algorithm {Name} (id={Id}) for well {WellId}. Timestamp: {Timestamp}", algorithm.Name, algorithm.Id, algorithm.WaterWellId, DateTimeOffset.UtcNow);

        switch (algorithm.Name)
        {
            case AlgorithmEnum.StaticAndDynamicLevel:
                await RunStaticDynamic(algorithm, ct);
                break;
            default:
                logger.LogWarning("Algorithm {Algo} is not implemented", algorithm.Name);
                break;
        }

        logger.LogInformation("Finished algorithm {Id}", algorithm.Id);
    }
    private async Task RunStaticDynamic(Algorithm algorithm, CancellationToken stoppingToken)
    {
        var settings = JsonSerializer.Deserialize<SettingsStaticDynamic>(algorithm.Settings)
            ?? throw new IllegalSettingsException($"Settings for algorithm {algorithm.Name} is null");
        int channelId = settings.ChannelId;
        int wellId = algorithm.WaterWellId;
        long from = DateTimeOffset.UtcNow.Subtract(algorithm.LookbackInterval).ToUnixTimeMilliseconds();
        long to = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var response = await _apiClient.GetAsync(
            $"/api/v1/wells/{wellId}/data-harvesters/channels/{channelId}?from={from}&to={to}", stoppingToken)
                .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Request to receive data for channel_id={ChannelId} failed with status {StatusCode}. Error message: {Error}",
                channelId,
                response.StatusCode,
                await response.Content.ReadAsStringAsync(stoppingToken)
                    .ConfigureAwait(false));
            return;
        }

        await using var networkStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var records = CsvSensorValueReader.ReadData(networkStream);
        var result = new WellLevelAnalyzer().Analyze(records);
        db.AnalysisResults.Add(new AnalysisResult
        {
            StartTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(from),
            EndTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(to),
            AlgorithmId = algorithm.Id,
            Result = JsonSerializer.Serialize(new ResultStaticDynamic(result.StaticLevel, result.DynamicLevel))
        });
        await db.SaveChangesAsync(stoppingToken);
        return;
    }
}
