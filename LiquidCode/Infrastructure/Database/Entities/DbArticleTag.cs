using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Связь между статьёй и тегом
/// </summary>
[PrimaryKey(nameof(ArticleId), nameof(TagId))]
public class DbArticleTag : ITimestamped
{
    public int ArticleId { get; init; }
    public DbArticle Article { get; init; } = null!;
    
    public int TagId { get; init; }
    public DbTag Tag { get; init; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
