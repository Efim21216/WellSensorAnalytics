using System;

namespace WellSensorAnalytics.Models.Entities.Jsons;

public class ResultStaticDynamic(double? staticLevel, double? dynamicLevel)
{
    public double? StaticLevel { get; set; } = staticLevel;
    public double? DynamicLevel { get; set; } = dynamicLevel;
    public int Version { get; set; } = 1;
}
