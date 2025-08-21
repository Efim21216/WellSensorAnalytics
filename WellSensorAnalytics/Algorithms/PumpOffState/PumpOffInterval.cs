namespace WellSensorAnalytics;

public class PumpOffInterval
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public override string ToString()
    {
        return $"Насос выключен с {StartTime:HH:mm:ss} по {EndTime:HH:mm:ss}";
    }
}
