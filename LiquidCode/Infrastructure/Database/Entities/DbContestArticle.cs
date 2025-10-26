using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Связь между контестом и статьёй
/// </summary>
[PrimaryKey(nameof(ContestId), nameof(ArticleId))]
public class DbContestArticle : ITimestamped
{
    public int ContestId { get; init; }
    public DbContest Contest { get; init; } = null!;
    
    public int ArticleId { get; init; }
    public DbArticle Article { get; init; } = null!;
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
