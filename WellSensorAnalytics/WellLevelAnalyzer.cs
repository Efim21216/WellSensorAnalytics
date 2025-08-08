using MathNet.Numerics.Statistics;

namespace WellSensorAnalytics;
/// <summary>
/// Алгоритм работает следующим образом: 
/// Сначала данные сглаживаются. Строится гистограмма по 10 корзинам. Находятся локальные максимумы по количеству точек,
/// среди них находятся 2 наиболее популярные корзины. У корзины есть края, середины этих двух корзин
/// считаются примерными уровнями (статический и динамический). После этого происходит уточнение. 
/// Находятся стабильные точки и распределяются к одному из примерных уровней. После этого для
/// диманического уровня берётся медиана точек, распределенных к нему, а для статического берётся
/// 80 перцентиль.
/// </summary>
public class AnalysisResult
{
    public double? StaticLevel { get; set; }
    public double? DynamicLevel { get; set; }

    public override string ToString()
    {
        var staticStr = StaticLevel.HasValue ? $"{StaticLevel.Value:F2}" : "не найден";
        var dynamicStr = DynamicLevel.HasValue ? $"{DynamicLevel.Value:F2}" : "не найден";
        return $"Статический уровень: {staticStr}\nДинамический уровень: {dynamicStr}";
    }
}
public class WellLevelAnalyzer
{
    private double _upperStabilityThreshold = 0.001;
    private double _lowerStabilityThreshold = -0.001;

    private readonly int _minimumPointCount = 50;
    private readonly int _binCount = 10;

    public AnalysisResult Analyze(List<SensorValue> data)
    {
        if (data == null || data.Count < 2)
        {
            return new AnalysisResult(); 
        }
        var smoothedLevels = AnalysisUtils.ExponentialSmooth(data, 0.2);
        var peaks = FindHistogramPeaks(smoothedLevels);

        if (peaks.Count < 2)
        {
            return new AnalysisResult();
        }

        double approxStaticLevel = Math.Max(peaks[0], peaks[1]);
        double approxDynamicLevel = Math.Min(peaks[0], peaks[1]);

        var stableStaticPoints = new List<double>();
        var stableDynamicPoints = new List<double>();
        _upperStabilityThreshold = Math.Max(_upperStabilityThreshold, Statistics.Percentile(smoothedLevels, 80));
        _lowerStabilityThreshold = Math.Min(_lowerStabilityThreshold, Statistics.Percentile(smoothedLevels, 5));
        System.Console.WriteLine($"Up: {_upperStabilityThreshold}, Low: {_lowerStabilityThreshold}");

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

        var result = new AnalysisResult
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
