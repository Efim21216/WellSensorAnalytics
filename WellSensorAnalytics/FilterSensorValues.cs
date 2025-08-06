namespace WellSensorAnalytics;

public static class FilterSensorValues
{
    public static List<SensorValue> AfterDateTime(DateTime startDate, List<SensorValue> records)
    {
        return records.Where(dp =>
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds(dp.EpochMilliseconds).UtcDateTime;
            return date >= startDate;
        }).ToList();
    }
}
