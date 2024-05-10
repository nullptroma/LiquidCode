using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Models.Database;

public class DbUserSubmit
{
    public int Id { get; set; }
    public DbUser User { get; init; } = null!;
    public DbSolution Solution { get; init; } = null!;
}