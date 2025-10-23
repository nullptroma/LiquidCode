using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность пользователя с улучшенной индексацией и полями аудита
/// </summary>
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email))]
[Index(nameof(IsDeleted))]
public class DbUser : ISoftDeletable, ITimestamped
{
    public int Id { get; init; }
    
    [StringLength(32, MinimumLength = 4)] 
    public string Username { get; init; } = "";
    
    [StringLength(256, MinimumLength = 4)] 
    public string Email { get; init; } = "";
    
    [StringLength(256)] 
    public string PassHash { get; init; } = "";
    
    [StringLength(512)] 
    public string Salt { get; init; } = "";
    
    // Soft delete support
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}