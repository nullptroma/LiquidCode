using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Реализация репозитория для операций с базой данных, связанных с миссиями
/// </summary>
public class MissionRepository : IMissionRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbMission> _missionRepository;

    public MissionRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _missionRepository = new DbCrud<DbMission>(dbContext);
    }

    // IRepository<DbMission> implementation (delegated to _missionRepository)
    public Task<DbMission?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _missionRepository.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbMission> Items, bool HasNextPage)> GetPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default) =>
        _missionRepository.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbMission entity, CancellationToken cancellationToken = default) =>
        _missionRepository.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbMission entity, CancellationToken cancellationToken = default) =>
        _missionRepository.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbMission entity, CancellationToken cancellationToken = default) =>
        _missionRepository.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbMission entity, CancellationToken cancellationToken = default) =>
        _missionRepository.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _missionRepository.SaveChangesAsync(cancellationToken);

    // IMissionRepository specific methods
    public async Task<IEnumerable<DbMission>> GetMissionsByAuthorAsync(int authorId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbMission>()
            .Where(m => m.Author.Id == authorId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<DbMission?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Missions
            .Include(m => m.Author)
            .Include(m => m.MissionTags)
                .ThenInclude(mt => mt.Tag)
            .Include(m => m.ContestEntries)
                .ThenInclude(cm => cm.Contest)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<(IEnumerable<DbMission> Items, bool HasNextPage)> GetFilteredPageAsync(
        int pageSize,
        int pageNumber,
        IEnumerable<int>? tagIds,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var query = _dbContext.Missions
            .Include(m => m.Author)
            .Include(m => m.MissionTags)
                .ThenInclude(mt => mt.Tag)
            .Where(m => !m.IsDeleted);

        if (tagIds != null)
        {
            var tagArray = tagIds.ToArray();
            if (tagArray.Length > 0)
            {
                query = query.Where(m => m.MissionTags.Any(mt => tagArray.Contains(mt.TagId)));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, hasNextPage);
    }

    public async Task SyncTagsAsync(DbMission mission, IEnumerable<int> tagIds, CancellationToken cancellationToken = default)
    {
        var targetIds = tagIds?.ToHashSet() ?? new HashSet<int>();

        var existing = await _dbContext.MissionTags
            .Where(mt => mt.MissionId == mission.Id)
            .ToListAsync(cancellationToken);

        var toRemove = existing.Where(e => !targetIds.Contains(e.TagId)).ToList();
        if (toRemove.Count > 0)
        {
            _dbContext.MissionTags.RemoveRange(toRemove);
        }

    var existingIds = existing.Select(e => e.TagId).ToHashSet();
        var toAdd = targetIds.Except(existingIds)
            .Select(tagId => new DbMissionTag
            {
                MissionId = mission.Id,
                TagId = tagId
            }).ToList();

        if (toAdd.Count > 0)
        {
            await _dbContext.MissionTags.AddRangeAsync(toAdd, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DbMissionPublicTextData?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default) =>
        await _dbContext.MissionsTextData
            .FirstOrDefaultAsync(m => m.MissionId == missionId && m.Language == language, cancellationToken);

    public async Task<IEnumerable<string>> GetMissionLanguagesAsync(int missionId, CancellationToken cancellationToken = default) =>
        await _dbContext.MissionsTextData
            .Where(m => m.MissionId == missionId)
            .Select(m => m.Language)
            .ToListAsync(cancellationToken);

    public async Task CreateMissionTextAsync(DbMissionPublicTextData textData, CancellationToken cancellationToken = default) =>
        await _dbContext.MissionsTextData.AddAsync(textData, cancellationToken);

    public async Task CreateMissionTextsAsync(IEnumerable<DbMissionPublicTextData> textData, CancellationToken cancellationToken = default) =>
        await _dbContext.MissionsTextData.AddRangeAsync(textData, cancellationToken);

    public async Task<int> CountMissionsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbMission>().CountAsync(cancellationToken);
}
