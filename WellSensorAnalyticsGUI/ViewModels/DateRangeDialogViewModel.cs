using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WellSensorAnalyticsGUI.Messages;
using WellSensorAnalyticsGUI.Models;

namespace WellSensorAnalyticsGUI.ViewModels;

public partial class DateRangeDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);

    [ObservableProperty]
    private DateTimeOffset _endDate = DateTimeOffset.Now;
    [ObservableProperty]
    private TimeSpan _startTime = TimeSpan.Zero;

    [ObservableProperty]
    private TimeSpan _endTime = TimeSpan.Zero;


    [RelayCommand]
    private void Ok()
    {
        WeakReferenceMessenger.Default.Send(new DateRangeCloseDialogMessage(new DateRange(StartDate.Add(StartTime), EndDate.Add(EndTime))));
    }
}
