using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Mission entity with improved indexing and audit fields
/// </summary>
[Index(nameof(Difficulty))]
[Index(nameof(CreatedAt))]
[Index(nameof(IsDeleted))]
public class DbMission : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    public DbUser Author { get; init; } = null!;
    
    [StringLength(128)] 
    public string Name { get; set; } = "";
    
    [StringLength(256)] 
    public string S3PrivateKey { get; init; } = "";
    
    public int Difficulty { get; init; }
    
    // Soft delete support
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}