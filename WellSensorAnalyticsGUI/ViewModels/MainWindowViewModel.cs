using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WellSensorAnalytics;
using WellSensorAnalyticsGUI.Messages;
using WellSensorAnalyticsGUI.Models;

namespace WellSensorAnalyticsGUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<string> DataFiles { get; } = [];
    public List<string> ListOfAlgorithms { get; } = [.. Enum.GetNames<Algorithms>()];

    [ObservableProperty]
    private string? _selectedFile;
    [ObservableProperty]
    private string? _selectedAlgorithm;
    [ObservableProperty]
    private string? _algorithmOutput = AppConstants.defaultAlgorithmOutput;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDateRangeCommand))]
    public List<SensorValue> _data = [];
    [ObservableProperty]
    public List<PumpOffInterval> _pumpOffIntervals = [];
    public void LoadDataFiles(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath).Select(a => Path.GetFileName(a));
        DataFiles.Clear();
        foreach (var file in files)
        {
            DataFiles.Add(file);
        }

    }
    partial void OnSelectedFileChanged(string? oldValue, string? newValue)
    {
        if (oldValue != newValue)
        {
            LoadData();
            ProcessAlgorithm(SelectedAlgorithm);
        }
    }
    [RelayCommand(CanExecute = nameof(CanProcessFile))]
    private async Task GetDateRangeAsync()
    {
        DateRange? dataRange = await WeakReferenceMessenger.Default.Send(new GetDateRangeMessage(
            DateTimeOffset.FromUnixTimeMilliseconds(Data.First().EpochMilliseconds).LocalDateTime,
            DateTimeOffset.FromUnixTimeMilliseconds(Data.Last().EpochMilliseconds).LocalDateTime
        ));
        if (dataRange == null) return;
        Data = FilterSensorValues.Between(dataRange.StartDate, dataRange.EndDate, Data);
    }
    partial void OnSelectedAlgorithmChanged(string? value)
    {
        ProcessAlgorithm(value);
    }

    private void ProcessAlgorithm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        if (Enum.TryParse(value, true, out Algorithms algorithm))
        {
            ClearAlgorithmState();
            switch (algorithm)
            {
                case Algorithms.StaticAndDynamicLevel:
                    AlgorithmOutput = new WellLevelAnalyzer().Analyze(Data).ToString();
                    break;
                case Algorithms.PumpOffState:
                    var analyzer = new PumpStateAnalyzer();
                    PumpOffIntervals = analyzer.DetectPumpOffIntervals(Data);
                    break;
            }
        }
    }
    private void ClearAlgorithmState()
    {
        AlgorithmOutput = AppConstants.defaultAlgorithmOutput;
        PumpOffIntervals = [];
    }
    public void LoadData()
    {
        Data = CsvSensorValueReader.ReadData(AppConstants.rootOfDataFiles + $"/{SelectedFile}");
    }
    private bool CanProcessFile()
    {
        return Data.Count > 0;
    }
}
