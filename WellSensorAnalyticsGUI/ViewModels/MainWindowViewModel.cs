using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WellSensorAnalytics;
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
            AlgorithmOutput = AppConstants.defaultAlgorithmOutput;
            ProcessAlgorithm(SelectedAlgorithm);
        }
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
}
