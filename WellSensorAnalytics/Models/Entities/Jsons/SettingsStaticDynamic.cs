using System;

namespace WellSensorAnalytics.Models.Entities.Jsons;

public class SettingsStaticDynamic
{
    public required int ChannelId { get; set; }

    public int Version { get; set; } = 1;
}
