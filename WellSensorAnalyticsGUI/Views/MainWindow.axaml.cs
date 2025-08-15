using System;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using ScottPlot.Avalonia;
using WellSensorAnalytics;
using WellSensorAnalyticsGUI.Messages;
using WellSensorAnalyticsGUI.Models;
using WellSensorAnalyticsGUI.ViewModels;

namespace WellSensorAnalyticsGUI.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        viewModel.LoadDataFiles(AppConstants.rootOfDataFiles);
        DataContext = viewModel;

        viewModel.PropertyChanged += ViewModelPropertyChanged;
        WeakReferenceMessenger.Default.Register<MainWindow, GetDateRangeMessage>(this, ShowDateRangePicker);
    }
    public static void ShowDateRangePicker(MainWindow w, GetDateRangeMessage m)
    {
        var viewModel = new DateRangeDialogViewModel();
        viewModel.StartDate = m.StartDateTime.Date;
        viewModel.EndDate = m.EndDateTime.Date;
        viewModel.StartTime = m.StartDateTime.TimeOfDay;
        viewModel.EndTime = m.EndDateTime.TimeOfDay;
        var dialog = new DateRangeDialogWindow
        {
            DataContext = viewModel
        };
        m.Reply(dialog.ShowDialog<DateRange?>(w));
    }
    private void ViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var viewModel = (MainWindowViewModel)DataContext!;
        if (e.PropertyName == nameof(MainWindowViewModel.Data))
        {
            UpdatePlot(viewModel);
        }
        if (e.PropertyName == nameof(MainWindowViewModel.PumpOffIntervals))
        {
            ProcessPumpOffIntervals(viewModel);
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
        DateTime[] xs = records.Select(v => DateTimeOffset.FromUnixTimeMilliseconds(v.EpochMilliseconds).LocalDateTime).ToArray();
        double[] ys = records.Select(v => v.Value).ToArray();

        var scatter = avaPlot.Plot.Add.Scatter(xs, ys, ScottPlot.Color.FromHex("#1f77b4"));
        ChartGenerator.ConfigureAppearance(avaPlot.Plot, scatter);
        avaPlot.Refresh();
        ProcessPumpOffIntervals(vm);
    }
}

