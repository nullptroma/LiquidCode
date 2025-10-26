using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Статья с контентом в S3 и тегами для навигации
/// </summary>
[Index(nameof(IsDeleted))]
public class DbArticle : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    public DbUser Author { get; init; } = null!;
    
    [StringLength(128)]
    public string Name { get; set; } = "";
    
    [StringLength(256)]
    public string S3ContentKey { get; init; } = "";
    
    public ICollection<DbArticleTag> ArticleTags { get; init; } = new HashSet<DbArticleTag>();
    public ICollection<DbContestArticle> ContestEntries { get; init; } = new HashSet<DbContestArticle>();
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
