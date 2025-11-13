using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность миссии с улучшенной индексацией и полями аудита
/// </summary>
[Index(nameof(Difficulty))]
[Index(nameof(CreatedAt))]
[Index(nameof(IsDeleted))]
public class DbMission : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    public DbUser Author { get; init; } = null!;
    
    [StringLength(ValidationLengths.Mission.NameMax, MinimumLength = ValidationLengths.Mission.NameMin)] 
    public string Name { get; set; } = "";
    
    [StringLength(ValidationLengths.Mission.S3KeyMax)] 
    public string S3Key { get; init; } = "";
    
    public int Difficulty { get; init; }

    public int? TimeLimitMilliseconds { get; set; }

    public int? MemoryLimitBytes { get; set; }
    
    public ICollection<DbMissionTag> MissionTags { get; init; } = new HashSet<DbMissionTag>();
    public ICollection<DbContestMission> ContestEntries { get; init; } = new HashSet<DbContestMission>();
    public ICollection<DbMissionStatement> Statements { get; init; } = new HashSet<DbMissionStatement>();
    
    // Поддержка мягкого удаления
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Временные метки
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}