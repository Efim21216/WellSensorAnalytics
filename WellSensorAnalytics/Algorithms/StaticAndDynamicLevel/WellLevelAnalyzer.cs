using MathNet.Numerics.Statistics;

namespace WellSensorAnalytics;

public class WellLevelAnalyzer
{
    private double _upperStabilityThreshold = 0.001;
    private double _lowerStabilityThreshold = -0.001;

    private readonly int _minimumPointCount = 50;
    private readonly int _binCount = 10;

    public WellLevelAnalysisResult Analyze(List<SensorValue> data)
    {
        if (data == null || data.Count < 2)
        {
            return new WellLevelAnalysisResult();
        }
        var smoothedLevels = AnalysisUtils.ExponentialSmooth(data, 0.2);
        var peaks = FindHistogramPeaks(smoothedLevels);

        if (peaks.Count < 2)
        {
            return new WellLevelAnalysisResult();
        }

        double approxStaticLevel = Math.Max(peaks[0], peaks[1]);
        double approxDynamicLevel = Math.Min(peaks[0], peaks[1]);

        var stableStaticPoints = new List<double>();
        var stableDynamicPoints = new List<double>();
        _upperStabilityThreshold = Math.Max(_upperStabilityThreshold, Statistics.Percentile(smoothedLevels, 80));
        _lowerStabilityThreshold = Math.Min(_lowerStabilityThreshold, Statistics.Percentile(smoothedLevels, 5));

        for (int i = 1; i < smoothedLevels.Length; i++)
        {
            var derivative = AnalysisUtils.CalculateRateOfChange(smoothedLevels, data, i);

            if ((derivative < _upperStabilityThreshold && derivative >= 0) ||
                (derivative > _lowerStabilityThreshold && derivative <= 0))
            {
                var currentLevel = smoothedLevels[i];
                if (Math.Abs(currentLevel - approxStaticLevel) < Math.Abs(currentLevel - approxDynamicLevel))
                {
                    stableStaticPoints.Add(currentLevel);
                }
                else
                {
                    stableDynamicPoints.Add(currentLevel);
                }
            }
        }

        var result = new WellLevelAnalysisResult
        {
            StaticLevel = stableStaticPoints.Count != 0 ? Statistics.Percentile(stableStaticPoints, 95) : null,
            DynamicLevel = stableDynamicPoints.Count != 0 ? Statistics.Median(stableDynamicPoints) : null
        };

        return result;
    }


    private List<double> FindHistogramPeaks(double[] data)
    {
        var binCount = 10;
        var buckets = new Histogram(data, binCount);
        // Поиск локальных максимумов
        var peakIndices = new List<int>();
        for (int i = 1; i < buckets.BucketCount - 1; i++)
        {
            if (buckets[i].Count > buckets[i - 1].Count && buckets[i].Count > buckets[i + 1].Count && buckets[i].Count > _minimumPointCount)
            {
                peakIndices.Add(i);
            }
        }

        // Проверяем края
        if (buckets[0].Count > buckets[1].Count && buckets[0].Count > _minimumPointCount)
            peakIndices.Insert(0, 0);
        if (buckets[_binCount - 1].Count > buckets[_binCount - 2].Count && buckets[_binCount - 1].Count > _minimumPointCount)
            peakIndices.Add(_binCount - 1);


        if (peakIndices.Count < 2) return new List<double>();

        var topPeaks = peakIndices.OrderByDescending(i => buckets[i]).Take(2).ToList();

        // Конвертируем индексы корзин обратно в значения уровня
        return topPeaks.Select(i => buckets.LowerBound + (i + 0.5) * (buckets.UpperBound - buckets.LowerBound) / buckets.BucketCount).ToList();
    }
}
