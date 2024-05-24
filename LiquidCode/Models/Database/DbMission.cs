using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Models.Database;

public class DbMission
{
    public int Id { get; set; }
    public DbUser Author { get; init; } = null!; 
    [StringLength(128)] public string Name { get; set; } = "";
    [StringLength(256)] public string S3PublicKey { get; init; } = "";
    [StringLength(256)] public string S3PrivateKey { get; init; } = "";
    public int Difficulty { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}