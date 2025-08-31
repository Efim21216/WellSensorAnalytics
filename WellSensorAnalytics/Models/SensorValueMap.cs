using CsvHelper.Configuration;

namespace WellSensorAnalytics;

public class SensorValueMap : ClassMap<SensorValue>
{
    public SensorValueMap()
    {
        Map(sv => sv.Value).Name("value");
        Map(sv => sv.Timestamp).Name("timestamp");
    }
}
