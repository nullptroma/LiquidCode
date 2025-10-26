using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Связующая таблица между миссией и тегом
/// </summary>
[PrimaryKey(nameof(MissionId), nameof(TagId))]
public class DbMissionTag : ITimestamped
{
    public int MissionId { get; init; }
    public DbMission Mission { get; init; } = null!;
    
    public int TagId { get; init; }
    public DbTag Tag { get; init; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
