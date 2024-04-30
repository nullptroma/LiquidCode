using LiquidCode.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Db;

public class LiquidDbContext : DbContext
{
    public LiquidDbContext(DbContextOptions<LiquidDbContext> options)
        : base(options)
    {
    }

    public DbSet<DbUser> Users { get; set; } = null!;
    public DbSet<DbRefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<DbMission> Missions { get; set; } = null!;
    public DbSet<DbMissionPublicTextData> MissionsTextData { get; set; } = null!;
}