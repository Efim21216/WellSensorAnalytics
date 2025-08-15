using System;

namespace WellSensorAnalyticsGUI.Models;

public class DateRange(DateTimeOffset startDate, DateTimeOffset endDate)
{
    public DateTimeOffset StartDate { get; } = startDate;
    public DateTimeOffset EndDate { get; } = endDate;
}
