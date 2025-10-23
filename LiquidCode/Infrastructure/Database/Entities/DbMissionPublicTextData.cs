using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность текстовых данных миссии с составным индексом
/// </summary>
[Index(nameof(MissionId), nameof(Language), IsUnique = true)]
[Index(nameof(Language))]
public class DbMissionPublicTextData : ITimestamped
{
    public int Id { get; init; }
    
    [Required] 
    public int? MissionId { get; init; }
    
    [StringLength(64)] 
    public string Language { get; init; } = "";
    
    [StringLength(30000)] 
    public string Data { get; init; } = "";
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}