using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Контест, объединяющий миссии и статьи в рамках временного окна
/// </summary>
[Index(nameof(StartsAt))]
[Index(nameof(EndsAt))]
[Index(nameof(IsDeleted))]
public class DbContest : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    [StringLength(128)]
    public string Name { get; set; } = "";
    
    [StringLength(1024)]
    public string? Description { get; set; }
    
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    
    public int? GroupId { get; set; }
    public DbGroup? Group { get; set; }
    
    public ICollection<DbContestMission> Missions { get; init; } = new HashSet<DbContestMission>();
    public ICollection<DbContestArticle> Articles { get; init; } = new HashSet<DbContestArticle>();
    public ICollection<DbContestMembership> Memberships { get; init; } = new HashSet<DbContestMembership>();
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
