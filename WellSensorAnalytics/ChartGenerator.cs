using ScottPlot;

namespace WellSensorAnalytics;

public static class ChartGenerator
{
    private static readonly int width = 2000;
    private static readonly int height = 1000;
    public static void PlotTimeSeriesWithIntervals(DateTime[] timestamps, double[] values, List<PumpOffInterval> intervals, string file)
    {
        Plot plot = new();
        var scatter = plot.Add.Scatter(timestamps, values);

        foreach (var interval in intervals)
        {
            var span = plot.Add.HorizontalSpan(interval.StartTime.ToOADate(), interval.EndTime.ToOADate());
            span.FillStyle.Color = Colors.Green.WithAlpha(40);
        }

        ConfigureAppearance(plot, scatter);
        plot.SavePng(file, width, height);
    }
    public static void DisplayHistogram(List<double> rates)
    {
        Plot myPlot = new();
        var hist = ScottPlot.Statistics.Histogram.WithBinCount(30, rates);

        var barPlot = myPlot.Add.Bars(hist.Bins, hist.Counts);

        // Size each bar slightly less than the width of a bin
        foreach (var bar in barPlot.Bars)
        {
            bar.Size = hist.FirstBinSize * .8;
        }
        myPlot.Axes.SetLimits(top: 50);
        myPlot.SavePng("demo-hist.png", 400, 300);
    }
    private static void ConfigureAppearance(Plot plot, ScottPlot.Plottables.Scatter scatter)
    {
        scatter.MarkerSize = 0;

        var axis = plot.Axes.DateTimeTicksBottom();
        var tickGen = (ScottPlot.TickGenerators.DateTimeAutomatic)axis.TickGenerator;
        tickGen.LabelFormatter = CustomDateFormatter;

        plot.Title("Уровень воды в скважине");
        plot.YLabel("Уровень воды");
        plot.XLabel("Время");
    }
    private static string CustomDateFormatter(DateTime dt)
    {
        bool isMidnight = dt is { Hour: 0, Minute: 0, Second: 0 };
        return isMidnight
            ? DateOnly.FromDateTime(dt).ToString()
            : TimeOnly.FromDateTime(dt).ToString();
    }
}
