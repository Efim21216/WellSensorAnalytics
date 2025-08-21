using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WellSensorAnalytics.Models.Entities;

[Table("algorithm")]
[Index(nameof(Name))]
public class Algorithm
{
    [Key]
    public int Id { get; set; }
    [Column(TypeName = "varchar(127)")]
    public required AlgorithmEnum Name { get; set; }
    [Column(TypeName = "jsonb")]
    public required string Settings { get; set; }
    public int WaterWellId { get; set; }
}
