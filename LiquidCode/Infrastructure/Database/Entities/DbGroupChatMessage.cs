using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сообщение чата группы
/// </summary>
[Index(nameof(GroupId))]
public class DbGroupChatMessage : ITimestamped
{
    public long Id { get; set; }

    public int GroupId { get; set; }
    public DbGroup Group { get; init; } = null!;

    public int AuthorId { get; set; }
    public DbUser Author { get; init; } = null!;

    [Column(TypeName = "text")]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
