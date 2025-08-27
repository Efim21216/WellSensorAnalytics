using System;

namespace WellSensorAnalytics.Models.Entities;

public interface IAuditable
{
    DateTimeOffset LastModified { get; set; }
}
