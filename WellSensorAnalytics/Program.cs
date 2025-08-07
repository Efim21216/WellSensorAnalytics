namespace WellSensorAnalytics
{
    class Project
    {
        static void Main(string[] args)
        {
            var records = CsvSensorValueReader.ReadData("../../../dump-105.csv");
            var startDate = new DateTime(2025, 8, 6, 0, 0, 0, DateTimeKind.Utc);
            var filteredRecords = FilterSensorValues.AfterDateTime(startDate, records);

            DateTime[] xs = filteredRecords.Select(v => DateTimeOffset.FromUnixTimeMilliseconds(v.EpochMilliseconds).UtcDateTime).ToArray();
            double[] ys = filteredRecords.Select(v => v.Value).ToArray();
            var analyzer = new PumpStateAnalyzer(new PumpStateAnalyzerSettings
            {
                PumpStartThreshold = -0.0005,
                PumpStopThreshold = 0.01,
                SmoothingWindowSize = 5
                
            }
            );
            var offIntervals = analyzer.DetectPumpOffIntervals(filteredRecords);

            ChartGenerator.PlotTimeSeriesWithIntervals(xs, ys, offIntervals, "demo2.png");
        }
    }
}
