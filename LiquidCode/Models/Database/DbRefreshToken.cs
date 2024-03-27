using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Db.Models;

public class DbRefreshToken
{
    [Key] public string Token { get; set; } = "";
    public DbUser DbUser { get; set; } = null!;
    public DateTime Expires { get; set; }
    [StringLength(512)] public string OsName { get; set; } = "";
    [StringLength(128)] public string IpAddress { get; set; } = "";
}