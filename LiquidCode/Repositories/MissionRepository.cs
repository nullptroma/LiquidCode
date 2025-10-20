using LiquidCode.Db;
using LiquidCode.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Repositories;

/// <summary>
/// Repository implementation for mission-related database operations
/// </summary>
public class MissionRepository : Repository<DbMission>, IMissionRepository
{
    public MissionRepository(LiquidDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IEnumerable<DbMission> Missions, bool HasNextPage)> GetMissionsPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var totalCount = await DbSet.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var missions = await DbSet
            .OrderBy(m => m.Id)
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (missions, hasNextPage);
    }

    public async Task<IEnumerable<DbMission>> GetMissionsByAuthorAsync(int authorId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(m => m.Author.Id == authorId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<DbMissionPublicTextData?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default) =>
        await DbContext.MissionsTextData
            .FirstOrDefaultAsync(m => m.MissionId == missionId && m.Language == language, cancellationToken);

    public async Task<IEnumerable<string>> GetMissionLanguagesAsync(int missionId, CancellationToken cancellationToken = default) =>
        await DbContext.MissionsTextData
            .Where(m => m.MissionId == missionId)
            .Select(m => m.Language)
            .ToListAsync(cancellationToken);

    public async Task AddMissionTextAsync(DbMissionPublicTextData textData, CancellationToken cancellationToken = default) =>
        await DbContext.MissionsTextData.AddAsync(textData, cancellationToken);

    public async Task AddMissionTextsAsync(IEnumerable<DbMissionPublicTextData> textData, CancellationToken cancellationToken = default) =>
        await DbContext.MissionsTextData.AddRangeAsync(textData, cancellationToken);

    public async Task<int> CountMissionsAsync(CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(cancellationToken);
}
