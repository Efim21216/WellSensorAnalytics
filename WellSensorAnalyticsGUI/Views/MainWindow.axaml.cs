using System;
using System.Linq;
using Avalonia.Controls;
using ScottPlot.Avalonia;
using WellSensorAnalytics;
using WellSensorAnalyticsGUI.ViewModels;

namespace WellSensorAnalyticsGUI.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }
    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(MainWindowViewModel.Data))
                    {
                        UpdatePlot(viewModel);
                    }
                    if (e.PropertyName == nameof(MainWindowViewModel.PumpOffIntervals))
                    {
                        ProcessPumpOffIntervals(viewModel);
                    }
                };
        }
    }
    private void ProcessPumpOffIntervals(MainWindowViewModel viewModel)
    {
        AvaPlot? avaPlot = this.Find<AvaPlot>("AvaPlot");
        if (avaPlot == null)
        {
            Console.WriteLine("AvaPlot is NULL!!!");
            return;
        }
        ChartGenerator.RemoveOffIntervals(avaPlot.Plot);
        var parseStatus = Enum.TryParse(viewModel.SelectedAlgorithm, true, out Algorithms algorithm);
        if (parseStatus && algorithm == Algorithms.PumpOffState)
        {
            ChartGenerator.DisplayOffIntervals(avaPlot.Plot, viewModel.PumpOffIntervals);
        }
        avaPlot.Refresh();
    }
    private void UpdatePlot(MainWindowViewModel vm)
    {
        AvaPlot? avaPlot = this.Find<AvaPlot>("AvaPlot");
        if (avaPlot == null)
        {
            Console.WriteLine("AvaPlot is NULL!!!");
            return;
        }
        avaPlot.Plot.Clear();
        
        var records = vm.Data;
        DateTime[] xs = records.Select(v => DateTimeOffset.FromUnixTimeMilliseconds(v.EpochMilliseconds).UtcDateTime).ToArray();
        double[] ys = records.Select(v => v.Value).ToArray();

        var scatter = avaPlot.Plot.Add.Scatter(xs, ys, ScottPlot.Color.FromHex("#1f77b4"));
        ChartGenerator.ConfigureAppearance(avaPlot.Plot, scatter);
        avaPlot.Refresh();
        ProcessPumpOffIntervals(vm);
    }
}
