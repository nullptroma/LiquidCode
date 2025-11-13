using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность токена обновления с индексацией для производительности
/// </summary>
[Index(nameof(Expires))]
public class DbRefreshToken : ITimestamped
{
    [Key] 
    [StringLength(ValidationLengths.RefreshToken.TokenMax)] 
    public string Token { get; init; } = "";
    
    public DbUser DbUser { get; init; } = null!;
    
    public DateTime Expires { get; init; }
    
    [StringLength(ValidationLengths.RefreshToken.OsNameMax)] 
    public string OsName { get; init; } = "";
    
    [StringLength(ValidationLengths.RefreshToken.IpAddressMax)] 
    public string IpAddress { get; init; } = "";
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}