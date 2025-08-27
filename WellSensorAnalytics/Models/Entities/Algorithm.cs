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
    public int WaterWellId { get; set; }
    public TimeSpan ScheduleInterval { get; set; }
    public bool Enabled { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public DateTimeOffset? LastRun { get; set; }
}
