using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            builder.Services.AddHostedService<Worker>();
        }
        static void ConfigureSourceOfSettings(HostApplicationBuilder builder)
        {
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Services.Configure<OAuthConfig>(builder.Configuration.GetSection("OAuthConfig"));
            builder.Configuration.AddUserSecrets<Project>();
            builder.Configuration.AddEnvironmentVariables();
        }
        static async Task TestDb()
        {
            var connectionString = "Host=localhost;Port=5432;Database=well_sensor_analytics;Username=postgres;Password=2122;";
            var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            using var db = new AnalyticsDbContext(optionsBuilder.Options);

            // Create
            Console.WriteLine("Inserting a new algo");
            string jsonString = """
                {
                    "P1": "str",
                    "P2": 30
                }
                """;
            db.Add(new Algorithm { WaterWellId = 1, Name = AlgorithmEnum.PumpOffState, Settings = jsonString });
            await db.SaveChangesAsync();

            // Read
            Console.WriteLine("Querying for a algo");
            var alg = await db.Algorithms
                .OrderBy(a => a.Id)
                .FirstAsync();

            // Update
            Console.WriteLine("Updating the alg and adding a result");
            alg.Settings = """
                {
                    "P1": "str",
                    "P2": 13,
                    "P3": true
                }
                """;
            db.AnalysisResults.Add(
                new AnalysisResult { Algorithm = alg, Result = """
                {
                    "result": 5
                }
                """ });
            await db.SaveChangesAsync();

            // Delete
            Console.WriteLine("Delete the alg");
            db.Remove(alg);
            await db.SaveChangesAsync();
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
