using System.Globalization;
using CsvHelper;

namespace WellSensorAnalytics;

public static class CsvSensorValueReader
{
    public static List<SensorValue> ReadData(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<SensorValueMap>();
        return csv.GetRecords<SensorValue>().ToList();
    }
}
