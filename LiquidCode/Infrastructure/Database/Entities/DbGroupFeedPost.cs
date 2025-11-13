using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Пост в ленте группы
/// </summary>
[Index(nameof(GroupId))]
[Index(nameof(IsDeleted))]
public class DbGroupFeedPost : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public DbGroup Group { get; init; } = null!;

    public int AuthorId { get; set; }
    public DbUser Author { get; init; } = null!;

    [StringLength(ValidationLengths.Group.FeedPostNameMax)]
    public string Name { get; set; } = "";

    [Column(TypeName = "text")]
    public string Content { get; set; } = "";

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
