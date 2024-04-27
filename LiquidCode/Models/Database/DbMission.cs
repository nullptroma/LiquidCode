using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LiquidCode.Models.Database;

public class DbMission
{
    public int Id { get; set; }
    [Required] public DbUser? Author { get; set; } = null;
    [StringLength(128)] public string Name { get; set; } = "";
    [StringLength(256)] public string S3FileName { get; set; } = "";
    public int Difficulty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}