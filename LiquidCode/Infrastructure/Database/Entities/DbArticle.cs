using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Статья с контентом в S3 и тегами для навигации
/// </summary>
[Index(nameof(IsDeleted))]
public class DbArticle : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    public int AuthorId { get; set; }
    public DbUser Author { get; init; } = null!;
    
    [StringLength(128)]
    public string Name { get; set; } = "";
    
    [Column(TypeName = "text")]
    public string Content { get; set; } = "";
    
    public ICollection<DbArticleTag> ArticleTags { get; init; } = new HashSet<DbArticleTag>();
    public ICollection<DbContestArticle> ContestEntries { get; init; } = new HashSet<DbContestArticle>();
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
