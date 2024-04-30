using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Models.Database;

public class DbRefreshToken
{
    [Key] [StringLength(128)] public string Token { get; init; } = "";
    public DbUser DbUser { get; init; } = null!;
    public DateTime Expires { get; init; }
    [StringLength(512)] public string OsName { get; init; } = "";
    [StringLength(128)] public string IpAddress { get; init; } = "";
}