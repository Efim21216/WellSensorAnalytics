using System;
using WellSensorAnalyticsGUI.Models;

namespace WellSensorAnalyticsGUI.Messages;

public class DateRangeCloseDialogMessage(DateRange dateRange)
{
    public DateRange DateRange { get; } = dateRange;
}
