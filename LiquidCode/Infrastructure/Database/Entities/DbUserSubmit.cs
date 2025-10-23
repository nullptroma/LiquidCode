using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность отправки пользователя с индексацией и временными метками
/// </summary>
[Index(nameof(CreatedAt))]
[Index(nameof(IsDeleted))]
public class DbUserSubmit : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    public DbUser User { get; init; } = null!;
    public DbSolution Solution { get; init; } = null!;
    
    // Soft delete support
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}