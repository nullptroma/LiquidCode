using System.ComponentModel.DataAnnotations;
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
    [StringLength(256)]
    public string FileName { get; set; } = "";
    
    /// <summary>
    /// Ключ файла в S3 (для публичного доступа)
    /// </summary>
    [StringLength(512)]
    public string MediaKey { get; set; } = "";
    
    /// <summary>
    /// URL для доступа к файлу в S3
    /// </summary>
    [StringLength(512)]
    public string MediaUrl { get; set; } = "";
    
    /// <summary>
    /// Порядок отображения файла среди других медиа в этом тексте
    /// </summary>
    public int OrderIndex { get; set; }
}
