namespace WellSensorAnalytics
{
    class Project
    {
        static void Main(string[] args)
        {
            //Ожидается, что записи отсортированы!
            var records = CsvSensorValueReader.ReadData("../../../dump-105.csv");
            var startDate = new DateTime(2025, 8, 6, 0, 0, 0, DateTimeKind.Utc);
            var filteredRecords = FilterSensorValues.AfterDateTime(startDate, records);

            System.Console.WriteLine(new WellLevelAnalyzer().Analyze(filteredRecords));
            FindPumpOnOffState(filteredRecords);
        }
        static void FindPumpOnOffState(List<SensorValue> filteredRecords)
        {
            DateTime[] xs = filteredRecords.Select(v => DateTimeOffset.FromUnixTimeMilliseconds(v.EpochMilliseconds).UtcDateTime).ToArray();
            double[] ys = filteredRecords.Select(v => v.Value).ToArray();
            var analyzer = new PumpStateAnalyzer();
            var offIntervals = analyzer.DetectPumpOffIntervals(filteredRecords);

            ChartGenerator.PlotTimeSeriesWithIntervals(xs, ys, offIntervals, "demo3.png");
        }
    }
}
