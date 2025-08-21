using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WellSensorAnalytics.Models.Entities;

[Table("analysis_result")]
public class AnalysisResult
{
    [Key]
    public int Id { get; set; }
    [Column(TypeName = "jsonb")]
    public required string Result { get; set; }
    public int AlgorithmId { get; set; }
    public Algorithm Algorithm { get; set; } = null!;
}
