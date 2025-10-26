using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Связь между контестом и миссией с порядком отображения
/// </summary>
[PrimaryKey(nameof(ContestId), nameof(MissionId))]
public class DbContestMission : ITimestamped
{
    public int ContestId { get; init; }
    public DbContest Contest { get; init; } = null!;
    
    public int MissionId { get; init; }
    public DbMission Mission { get; init; } = null!;
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
