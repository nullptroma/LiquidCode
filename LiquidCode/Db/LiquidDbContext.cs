using LiquidCode.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Db;

public class LiquidDbContext : DbContext
{
    public DbSet<DbUser> Users { get; set; } = null!;
    public DbSet<DbRefreshToken> RefreshTokens { get; set; } = null!;

    public LiquidDbContext(DbContextOptions<LiquidDbContext> options)
        : base(options)
    {
    }
}