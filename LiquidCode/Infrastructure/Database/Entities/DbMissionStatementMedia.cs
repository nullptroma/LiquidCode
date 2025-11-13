using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Медиа файл (картинка) связанный с текстом миссии
/// </summary>
[Index(nameof(StatementId))]
[Index(nameof(MediaKey))]
public class DbMissionStatementMedia
{
    public int Id { get; set; }
    
    public int StatementId { get; set; }
    public DbMissionStatement Statement { get; init; } = null!;
    
    /// <summary>
    /// Оригинальное имя файла (например: "o1.png", "15c12c02bcb2f87450906d26075f1336c6f8bb79.png")
    /// </summary>
    [StringLength(ValidationLengths.Mission.StatementMediaFileNameMax)]
    public string FileName { get; set; } = "";
    
    /// <summary>
    /// Ключ файла в S3 (для публичного доступа)
    /// </summary>
    [StringLength(ValidationLengths.Mission.StatementMediaKeyMax)]
    public string MediaKey { get; set; } = "";
    
    /// <summary>
    /// URL для доступа к файлу в S3
    /// </summary>
    [StringLength(ValidationLengths.Mission.StatementMediaUrlMax)]
    public string MediaUrl { get; set; } = "";
}
