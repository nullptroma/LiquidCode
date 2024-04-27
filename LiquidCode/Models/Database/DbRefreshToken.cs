using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Models.Database;

public class DbRefreshToken
{
    [Key] [StringLength(128)] public string Token { get; set; } = "";
    public DbUser DbUser { get; set; } = null!;
    public DateTime Expires { get; set; }
    [StringLength(512)] public string OsName { get; set; } = "";
    [StringLength(128)] public string IpAddress { get; set; } = "";
}