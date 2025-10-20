using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database;

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
    public DbSet<DbSolution> Solutions { get; set; } = null!;
    public DbSet<DbUserSubmit> UserSubmits { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure global query filters for soft delete
        modelBuilder.Entity<DbUser>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DbMission>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<DbUserSubmit>().HasQueryFilter(s => !s.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ITimestamped && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (ITimestamped)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}