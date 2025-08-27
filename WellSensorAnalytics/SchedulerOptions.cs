using System;

namespace WellSensorAnalytics;

public class SchedulerOptions
{
    public TimeSpan SyncInterval { get; set; } = TimeSpan.FromSeconds(30);
}
