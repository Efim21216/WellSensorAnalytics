using MathNet.Numerics.Statistics;

namespace WellSensorAnalytics;

public enum PumpState
{
    On,
    Off
}
public class PumpStateAnalyzerSettings
{
    // Порог скорости (м/сек) для определения ВКЛЮЧЕНИЯ насоса.
    // Если скорость падения уровня ниже этого значения, считаем, что насос включился.
    public double PumpStartThreshold { get; set; }
    // Порог скорости (м/сек) для определения ВЫКЛЮЧЕНИЯ насоса.
    // Если скорость роста уровня выше этого значения, считаем, что насос выключился.
    public double PumpStopThreshold { get; set; }
    // Коэффициент сглаживания для экспоненциального сглаживания. 
    // Значение от 0 до 1. Ближе к 1 — меньше сглаживания, ближе к 0 — больше.
    public double SmoothingAlpha { get; set; } = 0.2;
    //Требование, чтобы скорость изменения оставалась ниже PumpStartThreshold или 
    // выше PumpStopThreshold в течение нескольких последовательных точек
    public int MinConsecutivePoints { get; set; } = 5;
    public int LowerPercentile { get; set; } = 20;
    public int UpperPercentile { get; set; } = 95;
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
/// <summary>
/// Алгоритм основывается на наблюдении за скоростью изменения воды в скважине.
/// Есть порог включения и выключения, если скорость изменения из пересекает, то насос переходит
/// в соответствующее состояние.
/// Алгоритм работает следующим образом:
/// Сначала данные сглаживаются. После этого находим пороги для включения и выключения 
/// с использованием перцентилей. Дальше идём по точкам и анализируем скорость изменения.
/// Если она пересекает порог, то мы начинаем считать количество последовательных точек,
/// которые пересекут этот порог. Если количество достигает MinConsecutivePoints, то насос переходит
/// в новое состояние.
/// </summary>

public class PumpStateAnalyzer
{
    private readonly PumpStateAnalyzerSettings _settings;
    private int _onCounter;
    private int _offCounter;
    private DateTime? _potentialOffEndTime;
    private DateTime? _potentialOffStartTime;

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
        if (records.Count < 2)
        {
            return new List<PumpOffInterval>();
        }

        var results = new List<PumpOffInterval>();

        double[] smoothedValues = AnalysisUtils.ExponentialSmooth(records, _settings.SmoothingAlpha);
        List<double> rates = [];
        for (int i = 1; i < smoothedValues.Length; i++)
        {
            rates.Add(AnalysisUtils.CalculateRateOfChange(smoothedValues, records, i));
        }

        var (currentState, currentInterval) = InitializeState(rates[0], records[0].EpochMilliseconds);
        _settings.PumpStartThreshold = Math.Min(Statistics.Percentile(rates, _settings.LowerPercentile), -0.0001);
        _settings.PumpStopThreshold = Statistics.Percentile(rates, _settings.UpperPercentile);
        System.Console.WriteLine($"Start: {_settings.PumpStartThreshold}, Stop: {_settings.PumpStopThreshold}");
        // Определение состояний
        for (int i = 1; i < smoothedValues.Length; i++)
        {
            var rateOfChange = rates[i - 1];
            var previousRecord = records[i - 1];
            (currentState, currentInterval) = ProcessState(currentState, currentInterval, rateOfChange, previousRecord, results);
        }

        FinalizeInterval(currentInterval, records, results);

        return results;
    }


    private (PumpState, PumpOffInterval?) InitializeState(double initialRate, long startTimestamp)
    {
        if (initialRate > _settings.PumpStopThreshold)
        {
            return (
                PumpState.Off,
                new PumpOffInterval
                {
                    StartTime = DateTimeOffset.FromUnixTimeMilliseconds(startTimestamp).UtcDateTime
                }
            );
        }
        else
        {
            return (PumpState.On, null);
        }
    }

    private (PumpState, PumpOffInterval?) ProcessState(PumpState currentState, PumpOffInterval? currentInterval, double rateOfChange, SensorValue previousRecord, List<PumpOffInterval> results)
    {
        var nextState = currentState;
        var nextInterval = currentInterval;
        var currentTime = DateTimeOffset.FromUnixTimeMilliseconds(previousRecord.EpochMilliseconds).UtcDateTime;

        if (currentState == PumpState.Off)
        {
            // Ищем момент, когда насос ВКЛЮЧАЕТСЯ
            if (rateOfChange < _settings.PumpStartThreshold)
            {
                if (_onCounter == 0) _potentialOffEndTime = currentTime;
                _onCounter++;
                if (_onCounter >= _settings.MinConsecutivePoints)
                {
                    if (nextInterval != null)
                    {
                        nextInterval.EndTime = _potentialOffEndTime!.Value;
                        results.Add(nextInterval);
                        nextInterval = null;
                    }
                    nextState = PumpState.On;
                    _onCounter = 0;
                    _offCounter = 0;
                    _potentialOffEndTime = null;
                }
            }
            else
            {
                _onCounter = 0;
                _potentialOffEndTime = null;
            }
        }
        else
        {
            // Ищем момент, когда насос ВЫКЛЮЧАЕТСЯ
            if (rateOfChange > _settings.PumpStopThreshold)
            {
                if (_offCounter == 0) _potentialOffStartTime = currentTime;
                _offCounter++;
                if (_offCounter >= _settings.MinConsecutivePoints)
                {
                    nextInterval = new PumpOffInterval
                    {
                        StartTime = _potentialOffStartTime!.Value
                    };
                    nextState = PumpState.Off;
                    _offCounter = 0;
                    _onCounter = 0;
                    _potentialOffStartTime = null;
                }
            }
            else
            {
                _offCounter = 0;
                _potentialOffStartTime = null;
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
