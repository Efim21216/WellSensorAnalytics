using System.Globalization;
using CsvHelper;

namespace WellSensorAnalytics;

public static class CsvSensorValueReader
{
    public static List<SensorValue> ReadData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: The file was not found at {filePath}");
            return [];
        }
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<SensorValueMap>();
        return csv.GetRecords<SensorValue>().ToList();
    }
}
