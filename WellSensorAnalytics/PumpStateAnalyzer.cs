using MathNet.Numerics.Statistics;

namespace WellSensorAnalytics;

public enum PumpState
{
    On,
    Off
}
public class PumpStateAnalyzerSettings
{
    // Размер окна для сглаживания данных (количество точек). 
    public int SmoothingWindowSize { get; set; } = 5;
    // Порог скорости (м/сек) для определения ВКЛЮЧЕНИЯ насоса.
    // Если скорость падения уровня ниже этого значения, считаем, что насос включился.
    public double PumpStartThreshold { get; set; } = -0.005;
    // Порог скорости (м/сек) для определения ВЫКЛЮЧЕНИЯ насоса.
    // Если скорость роста уровня выше этого значения, считаем, что насос выключился.
    public double PumpStopThreshold { get; set; } = 0.005;
    public double SmoothingAlpha { get; set; } = 0.2;
}
public class PumpOffInterval
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public override string ToString()
    {
        return $"Насос выключен с {StartTime:HH:mm:ss} по {EndTime:HH:mm:ss}";
    }
}

public class PumpStateAnalyzer
{
    private readonly PumpStateAnalyzerSettings _settings;

    public PumpStateAnalyzer(PumpStateAnalyzerSettings settings)
    {
        _settings = settings;
    }
    public PumpStateAnalyzer()
    {
        _settings = new PumpStateAnalyzerSettings();
    }

    public List<PumpOffInterval> DetectPumpOffIntervals(List<SensorValue> records)
    {
        if (records.Count < _settings.SmoothingWindowSize)
        {
            return new List<PumpOffInterval>();
        }

        var results = new List<PumpOffInterval>();

        double[] smoothedValues = ExponentialSmooth(records);

        var (currentState, currentInterval) = InitializeState(records, smoothedValues);

        // Определение состояний
        for (int i = 1; i < smoothedValues.Length; i++)
        {
            var rateOfChange = CalculateRateOfChange(smoothedValues, records, i);
            var previousRecord = records[i - 1];
            (currentState, currentInterval) = ProcessState(currentState, currentInterval, rateOfChange, previousRecord, results);
        }

        FinalizeInterval(currentInterval, records, results);

        return results;
    }
    private double[] AverageSmooth(List<SensorValue> records)
    {
        double[] originalValues = records.Select(r => r.Value).ToArray();
        return Statistics.MovingAverage(originalValues, _settings.SmoothingWindowSize).ToArray();
    }
    private double[] ExponentialSmooth(List<SensorValue> records)
    {
        double[] data = records.Select(r => r.Value).ToArray();
        var smoothed = new double[data.Length];
        if (data.Length == 0) return smoothed;

        smoothed[0] = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            smoothed[i] = _settings.SmoothingAlpha * data[i] + (1 - _settings.SmoothingAlpha) * smoothed[i - 1];
        }
        return smoothed;
    }
    private (PumpState, PumpOffInterval?) InitializeState(List<SensorValue> records, double[] smoothedValues)
    {
        var initialRate = CalculateRateOfChange(smoothedValues, records, _settings.SmoothingWindowSize);
        if (initialRate > _settings.PumpStopThreshold)
        {
            return (
                PumpState.Off,
                new PumpOffInterval
                {
                    StartTime = DateTimeOffset.FromUnixTimeMilliseconds(records[0].EpochMilliseconds).UtcDateTime
                }
            );
        }
        else
        {
            return (PumpState.On, null);
        }
    }
    private static double CalculateRateOfChange(double[] smoothedValues, List<SensorValue> records, int idx)
    {
        var previousRecord = records[idx - 1];
        var currentRecord = records[idx];

        double valueDelta = smoothedValues[idx] - smoothedValues[idx - 1];
        double timeDeltaSeconds = (currentRecord.EpochMilliseconds - previousRecord.EpochMilliseconds) / 1000.0;

        // Пропускаем, если временной интервал нулевой
        if (timeDeltaSeconds == 0)
        {
            return 0;
        }

        return valueDelta / timeDeltaSeconds;
    }
    private (PumpState, PumpOffInterval?) ProcessState(PumpState currentState, PumpOffInterval? currentInterval, double rateOfChange, SensorValue previousRecord, List<PumpOffInterval> results)
    {
        var nextState = currentState;
        var nextInterval = currentInterval;

        if (currentState == PumpState.Off)
        {
            // Ищем момент, когда насос ВКЛЮЧАЕТСЯ
            if (rateOfChange < _settings.PumpStartThreshold)
            {
                if (nextInterval != null)
                {
                    nextInterval.EndTime = DateTimeOffset.FromUnixTimeMilliseconds(previousRecord.EpochMilliseconds).UtcDateTime;
                    results.Add(nextInterval);
                    nextInterval = null;
                }
                nextState = PumpState.On;
            }
        }
        else
        {
            // Ищем момент, когда насос ВЫКЛЮЧАЕТСЯ
            if (rateOfChange > _settings.PumpStopThreshold)
            {
                nextInterval = new PumpOffInterval
                {
                    StartTime = DateTimeOffset.FromUnixTimeMilliseconds(previousRecord.EpochMilliseconds).UtcDateTime
                };
                nextState = PumpState.Off;
            }
        }
        return (nextState, nextInterval);
    }
    private void FinalizeInterval(PumpOffInterval? currentInterval, List<SensorValue> records, List<PumpOffInterval> results)
    {
        if (currentInterval != null && currentInterval.EndTime == default)
        {
            currentInterval.EndTime = DateTimeOffset.FromUnixTimeMilliseconds(records.Last().EpochMilliseconds).UtcDateTime;
            results.Add(currentInterval);
        }
    }
}
