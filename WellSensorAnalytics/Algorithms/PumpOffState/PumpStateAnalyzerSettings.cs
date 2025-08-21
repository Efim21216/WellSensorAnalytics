namespace WellSensorAnalytics;

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
