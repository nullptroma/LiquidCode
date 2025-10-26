using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность отправки пользователя с индексацией и временными метками
/// </summary>
[Index(nameof(CreatedAt))]
[Index(nameof(IsDeleted))]
public class DbUserSubmission : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    public DbUser User { get; init; } = null!;
    public DbSolution Solution { get; init; } = null!;
    
    public int? ContestId { get; set; }
    public DbContest? Contest { get; set; }
    
    public SubmissionSourceType SourceType { get; set; } = SubmissionSourceType.Direct;
    
    // Поддержка мягкого удаления
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Временные метки
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Контекст, в котором была отправлена попытка
/// </summary>
public enum SubmissionSourceType
{
    Direct = 0,
    Contest = 1,
    ContestFlexibleWindow = 2
}