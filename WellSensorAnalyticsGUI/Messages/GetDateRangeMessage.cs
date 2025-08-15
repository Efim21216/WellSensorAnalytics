using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using WellSensorAnalyticsGUI.Models;

namespace WellSensorAnalyticsGUI.Messages;

public class GetDateRangeMessage(DateTimeOffset start, DateTimeOffset end) : AsyncRequestMessage<DateRange?>
{
    public DateTimeOffset StartDateTime { get; } = start;
    public DateTimeOffset EndDateTime { get; } = end;
}
