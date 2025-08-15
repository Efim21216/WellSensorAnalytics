using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using WellSensorAnalyticsGUI.Messages;

namespace WellSensorAnalyticsGUI.Views;

public partial class DateRangeDialogWindow : Window
{
    public DateRangeDialogWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<DateRangeDialogWindow, DateRangeCloseDialogMessage>(this,
                static (w, m) => w.Close(m.DateRange));
    }
}
