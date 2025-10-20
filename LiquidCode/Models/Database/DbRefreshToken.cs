using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Models.Database;

/// <summary>
/// Refresh token entity with indexing for performance
/// </summary>
[Index(nameof(Expires))]
public class DbRefreshToken : ITimestamped
{
    [Key] 
    [StringLength(128)] 
    public string Token { get; init; } = "";
    
    public DbUser DbUser { get; init; } = null!;
    
    public DateTime Expires { get; init; }
    
    [StringLength(512)] 
    public string OsName { get; init; } = "";
    
    [StringLength(128)] 
    public string IpAddress { get; init; } = "";
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}