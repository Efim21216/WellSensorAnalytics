using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellSensorAnalytics.Models.Entities;

[Table("algorithm")]
public class Algorithm : IAuditable
{

    [Key]
    public int Id { get; set; }
    [Column(TypeName = "varchar(127)")]
    public required AlgorithmEnum Name { get; set; }
    [Column(TypeName = "jsonb")]
    public required string Settings { get; set; }
    public required int WaterWellId { get; set; }
    public required TimeSpan ScheduleInterval { get; set; }
    public required TimeSpan LookbackInterval { get; set; }
    public bool Enabled { get; set; }
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastRun { get; set; }
}
