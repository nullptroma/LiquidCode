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
