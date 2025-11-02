using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using LiquidCode.Domain.Enums;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Текст миссии на конкретном языке с примерами и описанием
/// </summary>
[Index(nameof(MissionId))]
[Index(nameof(Language))]
[Index(nameof(MissionId), nameof(Language), nameof(Format), IsUnique = true)]
public class DbMissionStatement : ITimestamped
{
    public int Id { get; set; }
    
    public int MissionId { get; set; }
    public DbMission Mission { get; init; } = null!;
    
    /// <summary>
    /// Код языка (например: "russian", "english")
    /// </summary>
    [StringLength(50)]
    public string Language { get; set; } = "";

    /// <summary>
    /// Формат исходных файлов (Latex, Html и т.д.)
    /// </summary>
    public StatementFormat Format { get; set; } = StatementFormat.Latex;
    
    /// <summary>
    /// Текстовые файлы формулировки (problem.tex, input.tex, example.01 и т.д.)
    /// Сохраняется в JSON в БД, но работаем как со словарем
    /// </summary>
    public Dictionary<string, string> StatementTexts { get; set; } = new();
    
    /// <summary>
    /// Медиа файлы (картинки) связанные с этим текстом
    /// </summary>
    public ICollection<DbMissionStatementMedia> MediaFiles { get; init; } = new HashSet<DbMissionStatementMedia>();
    
    // Временные метки
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
