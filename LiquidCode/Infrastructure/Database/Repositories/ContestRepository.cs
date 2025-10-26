using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Репозиторий для работы с контестами
/// </summary>
public class ContestRepository : IContestRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbContest> _crud;

    public ContestRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _crud = new DbCrud<DbContest>(dbContext);
    }

    public Task<DbContest?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _crud.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetPageAsync(
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default) =>
        _crud.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbContest entity, CancellationToken cancellationToken = default) =>
        _crud.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbContest entity, CancellationToken cancellationToken = default) =>
        _crud.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbContest entity, CancellationToken cancellationToken = default) =>
        _crud.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbContest entity, CancellationToken cancellationToken = default) =>
        _crud.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _crud.SaveChangesAsync(cancellationToken);

    public async Task<DbContest?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Contests
            .Include(c => c.Group)
            .Include(c => c.Missions)
                .ThenInclude(cm => cm.Mission)
                    .ThenInclude(m => m.MissionTags)
                        .ThenInclude(mt => mt.Tag)
            .Include(c => c.Articles)
                .ThenInclude(ca => ca.Article)
                    .ThenInclude(a => a.ArticleTags)
                        .ThenInclude(at => at.Tag)
            .Include(c => c.Memberships)
                .ThenInclude(cm => cm.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetUpcomingAsync(
        int pageSize,
        int pageNumber,
        DateTime? from,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var startPoint = from ?? DateTime.UtcNow;
        var query = _dbContext.Contests
            .Include(c => c.Group)
            .Include(c => c.Memberships).ThenInclude(m => m.User)
            .Include(c => c.Missions).ThenInclude(cm => cm.Mission)
            .Include(c => c.Articles).ThenInclude(ca => ca.Article)
            .Where(c => !c.IsDeleted &&
                        ((c.ScheduleType == ContestScheduleType.FixedWindow && c.EndsAt >= startPoint) ||
                         (c.ScheduleType == ContestScheduleType.FlexibleWindow && c.AvailableUntil >= startPoint)))
            .OrderBy(c => c.ScheduleType)
            .ThenBy(c => c.ScheduleType == ContestScheduleType.FixedWindow ? c.StartsAt : c.AvailableFrom);

        var totalCount = await query.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var items = await query
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, hasNextPage);
    }

    public async Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetByGroupAsync(
        int groupId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var query = _dbContext.Contests
            .Include(c => c.Group)
            .Include(c => c.Memberships).ThenInclude(m => m.User)
            .Include(c => c.Missions).ThenInclude(cm => cm.Mission)
            .Include(c => c.Articles).ThenInclude(ca => ca.Article)
            .Where(c => c.GroupId == groupId && !c.IsDeleted)
            .OrderByDescending(c => c.ScheduleType == ContestScheduleType.FixedWindow ? c.StartsAt : c.AvailableFrom)
            .ThenByDescending(c => c.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var items = await query
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, hasNextPage);
    }

    public async Task SyncMissionsAsync(DbContest contest, IEnumerable<int> missionIds, CancellationToken cancellationToken = default)
    {
        var targetList = missionIds?.ToList() ?? new List<int>();
        var targetIds = targetList.ToHashSet();
        var existing = await _dbContext.ContestMissions
            .Where(cm => cm.ContestId == contest.Id)
            .ToListAsync(cancellationToken);

        var toRemove = existing.Where(e => !targetIds.Contains(e.MissionId)).ToList();
        if (toRemove.Count > 0)
        {
            _dbContext.ContestMissions.RemoveRange(toRemove);
        }

        var existingMap = existing.ToDictionary(e => e.MissionId);

        var toAdd = new List<DbContestMission>();
        for (var index = 0; index < targetList.Count; index++)
        {
            var missionId = targetList[index];
            if (existingMap.TryGetValue(missionId, out var entry))
            {
                entry.SortOrder = index;
            }
            else
            {
                toAdd.Add(new DbContestMission
                {
                    ContestId = contest.Id,
                    MissionId = missionId,
                    SortOrder = index
                });
            }
        }

        if (toAdd.Count > 0)
        {
            await _dbContext.ContestMissions.AddRangeAsync(toAdd, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncArticlesAsync(DbContest contest, IEnumerable<int> articleIds, CancellationToken cancellationToken = default)
    {
        var targetList = articleIds?.ToList() ?? new List<int>();
        var targetIds = targetList.ToHashSet();
        var existing = await _dbContext.ContestArticles
            .Where(ca => ca.ContestId == contest.Id)
            .ToListAsync(cancellationToken);

        var toRemove = existing.Where(e => !targetIds.Contains(e.ArticleId)).ToList();
        if (toRemove.Count > 0)
        {
            _dbContext.ContestArticles.RemoveRange(toRemove);
        }

        var existingMap = existing.ToDictionary(e => e.ArticleId);

        var toAdd = new List<DbContestArticle>();
        for (var index = 0; index < targetList.Count; index++)
        {
            var articleId = targetList[index];
            if (existingMap.TryGetValue(articleId, out var entry))
            {
                entry.SortOrder = index;
            }
            else
            {
                toAdd.Add(new DbContestArticle
                {
                    ContestId = contest.Id,
                    ArticleId = articleId,
                    SortOrder = index
                });
            }
        }

        if (toAdd.Count > 0)
        {
            await _dbContext.ContestArticles.AddRangeAsync(toAdd, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertMembershipAsync(int contestId, int userId, ContestMembershipRole role, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.ContestMemberships
            .FirstOrDefaultAsync(m => m.ContestId == contestId && m.UserId == userId, cancellationToken);

        if (membership == null)
        {
            membership = new DbContestMembership
            {
                ContestId = contestId,
                UserId = userId,
                Role = role
            };
            await _dbContext.ContestMemberships.AddAsync(membership, cancellationToken);
        }
        else
        {
            membership.Role = role;
            _dbContext.ContestMemberships.Update(membership);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMembershipAsync(int contestId, int userId, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.ContestMemberships
            .FirstOrDefaultAsync(m => m.ContestId == contestId && m.UserId == userId, cancellationToken);

        if (membership != null)
        {
            _dbContext.ContestMemberships.Remove(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<DbContestMembership?> GetMembershipAsync(int contestId, int userId, CancellationToken cancellationToken = default) =>
        _dbContext.ContestMemberships
            .FirstOrDefaultAsync(m => m.ContestId == contestId && m.UserId == userId, cancellationToken);
}
