using System;

namespace WellSensorAnalytics;

public static class AnalysisUtils
{
    public static double CalculateRateOfChange(double[] smoothedValues, List<SensorValue> records, int idx)
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
    public static double[] ExponentialSmooth(List<SensorValue> records, double smoothingAlpha)
    {
        double[] data = records.Select(r => r.Value).ToArray();
        var smoothed = new double[data.Length];
        if (data.Length == 0) return smoothed;

        smoothed[0] = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            smoothed[i] = smoothingAlpha * data[i] + (1 - smoothingAlpha) * smoothed[i - 1];
        }
        return smoothed;
    }
}
