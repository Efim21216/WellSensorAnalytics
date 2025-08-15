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
    public static List<SensorValue> Between(DateTimeOffset start, DateTimeOffset end, List<SensorValue> records)
    {
        return records.Where(dp =>
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds(dp.EpochMilliseconds).UtcDateTime;
            return date >= start && date <= end;
        }).ToList();
    }
}
