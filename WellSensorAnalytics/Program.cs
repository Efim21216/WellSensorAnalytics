using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WellSensorAnalytics.Algorithms;
using WellSensorAnalytics.Authentication;
using WellSensorAnalytics.Data;
using WellSensorAnalytics.Models.Entities;

namespace WellSensorAnalytics
{
    class Project
    {
        private static readonly bool isInDocker = false;
        static void Main(string[] args)
        {
            try
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
                ConfigureSourceOfSettings(builder);
                RegisterServices(builder);

                IHost host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fatal error: {ex}");
                Environment.Exit(1);
            }
        }
        static void RegisterServices(HostApplicationBuilder builder)
        {
            //DB
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContextPool<AnalyticsDbContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });
            builder.Services.AddTransient<IAlgorithmRepository, AlgorithmRepository>();

            //Network
            builder.Services.AddTransient<RefreshTokenHandler>();
            builder.Services.AddSingleton<ITokenRepository, InMemoryTokenRepository>();
            builder.Services.AddHttpClient<IAuthService, OAuth2Service>();
            var apiSettings = builder.Configuration.GetSection("ApiSettings");
            builder.Services.AddHttpClient("ApiClient", client =>
                {
                    client.BaseAddress = new Uri(apiSettings["BaseUrl"]!);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })
                .AddHttpMessageHandler<RefreshTokenHandler>();
            //Scheduler
            builder.Services.Configure<SchedulerOptions>(opt =>
            {
                opt.SyncInterval = TimeSpan.FromSeconds(5); // как часто синхронизировать с БД
            });
            builder.Services.AddSingleton<IAlgorithmRunner, AlgorithmRunnerStub>();
            builder.Services.AddHostedService<SchedulerService>();
        }
        static void ConfigureSourceOfSettings(HostApplicationBuilder builder)
        {
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Services.Configure<OAuthConfig>(builder.Configuration.GetSection("OAuthConfig"));
            builder.Configuration.AddUserSecrets<Project>();
            builder.Configuration.AddEnvironmentVariables();
        }
        static void runAnalyses()
        {
            //Ожидается, что записи отсортированы!
            var records = CsvSensorValueReader.ReadData(isInDocker ?
                "data/dump-105.csv" :
                "../../../../data-csv/dump-105.csv");
            var startDate = new DateTime(2025, 8, 6, 0, 0, 0, DateTimeKind.Utc);
            var filteredRecords = FilterSensorValues.AfterDateTime(startDate, records);
            if (filteredRecords.Count < 2)
            {
                Console.WriteLine("Lack of data");
                return;
            }
            Console.WriteLine(new WellLevelAnalyzer().Analyze(filteredRecords));
            FindPumpOnOffState(filteredRecords);
        }
        static void FindPumpOnOffState(List<SensorValue> filteredRecords)
        {
            DateTime[] xs = filteredRecords.Select(v => DateTimeOffset.FromUnixTimeMilliseconds(v.EpochMilliseconds).UtcDateTime).ToArray();
            double[] ys = filteredRecords.Select(v => v.Value).ToArray();
            var analyzer = new PumpStateAnalyzer();
            var offIntervals = analyzer.DetectPumpOffIntervals(filteredRecords);

            ChartGenerator.PlotTimeSeriesWithIntervals(xs, ys, offIntervals, isInDocker ? "" : "demo3.png");
        }
    }
}
