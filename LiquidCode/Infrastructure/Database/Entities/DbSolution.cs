using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Solution entity with indexing and timestamps
/// </summary>
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class DbSolution : ITimestamped
{
    public int Id { get; set; }
    
    [Required] 
    public DbMission Mission { get; init; } = null!;
    
    [StringLength(16)] 
    [Required] 
    public string Language { get; init; } = null!;
    
    [StringLength(16)] 
    [Required] 
    public string LanguageVersion { get; init; } = null!;
    
    [StringLength(10000)] 
    [Required] 
    public string SourceCode { get; init; } = null!;
    
    [StringLength(32)] 
    [Required] 
    public string Status { get; set; } = null!;
    
    public DateTime Time { get; init; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}