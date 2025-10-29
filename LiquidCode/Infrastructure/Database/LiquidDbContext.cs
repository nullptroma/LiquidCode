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
    public DbSet<DbMissionStatement> MissionStatements { get; set; } = null!;
    public DbSet<DbMissionStatementMedia> MissionStatementMedias { get; set; } = null!;
    public DbSet<DbSolution> Solutions { get; set; } = null!;
    public DbSet<DbUserSubmission> UserSubmits { get; set; } = null!;
    public DbSet<DbArticle> Articles { get; set; } = null!;
    public DbSet<DbTag> Tags { get; set; } = null!;
    public DbSet<DbMissionTag> MissionTags { get; set; } = null!;
    public DbSet<DbArticleTag> ArticleTags { get; set; } = null!;
    public DbSet<DbContest> Contests { get; set; } = null!;
    public DbSet<DbContestMission> ContestMissions { get; set; } = null!;
    public DbSet<DbContestArticle> ContestArticles { get; set; } = null!;
    public DbSet<DbContestMembership> ContestMemberships { get; set; } = null!;
    public DbSet<DbGroup> Groups { get; set; } = null!;
    public DbSet<DbGroupMembership> GroupMemberships { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Value converter для Dictionary<string, string> в DbMissionStatement
        var dictionaryConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Dictionary<string, string>, string>(
            v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions { WriteIndented = false }),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new Dictionary<string, string>()
        );

        var dictionaryComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<string, string>>(
            (c1, c2) => c1 != null && c2 != null ? c1.SequenceEqual(c2) : c1 == c2,
            c => c.Aggregate(0, (acc, kvp) => unchecked(acc * 397 ^ kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode())),
            c => c.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );

        var property = modelBuilder
            .Entity<DbMissionStatement>()
            .Property(e => e.StatementTexts);
            
        property.HasConversion(dictionaryConverter);
        property.Metadata.SetValueComparer(dictionaryComparer);
        property.HasColumnName("StatementTextsJson");
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