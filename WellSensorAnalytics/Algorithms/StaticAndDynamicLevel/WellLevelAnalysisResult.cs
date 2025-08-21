namespace WellSensorAnalytics;

/// <summary>
/// Алгоритм работает следующим образом: 
/// Сначала данные сглаживаются. Строится гистограмма по 10 корзинам. Находятся локальные максимумы по количеству точек,
/// среди них находятся 2 наиболее популярные корзины. У корзины есть края, середины этих двух корзин
/// считаются примерными уровнями (статический и динамический). После этого происходит уточнение. 
/// Находятся стабильные точки и распределяются к одному из примерных уровней. После этого для
/// динамического уровня берётся медиана точек, распределенных к нему, а для статического берётся
/// 80 персентиль.
/// </summary>
public class WellLevelAnalysisResult
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
