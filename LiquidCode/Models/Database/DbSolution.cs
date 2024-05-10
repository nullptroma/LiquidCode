using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Models.Database;

public class DbSolution
{
    public int Id { get; set; }
    [Required] public DbMission Mission { get; init; } = null!;
    [StringLength(16)] [Required] public string Language { get; init; } = null!;
    [StringLength(16)] [Required] public string LanguageVersion { get; init; } = null!;
    [StringLength(10000)] [Required] public string SourceCode { get; init; } = null!;
    [StringLength(32)] [Required] public string Status { get; set; } = null!;
    public DateTime Time { get; init; }
}