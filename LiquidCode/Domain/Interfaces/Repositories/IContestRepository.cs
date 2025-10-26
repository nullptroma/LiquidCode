using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для управления контестами
/// </summary>
public interface IContestRepository : IRepository<DbContest>
{
    Task<DbContest?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetUpcomingAsync(
        int pageSize,
        int pageNumber,
        DateTime? from,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbContest> Items, bool HasNextPage)> GetByGroupAsync(
        int groupId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task SyncMissionsAsync(DbContest contest, IEnumerable<int> missionIds, CancellationToken cancellationToken = default);
    Task SyncArticlesAsync(DbContest contest, IEnumerable<int> articleIds, CancellationToken cancellationToken = default);

    Task UpsertMembershipAsync(int contestId, int userId, ContestMembershipRole role, CancellationToken cancellationToken = default);
    Task RemoveMembershipAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task<DbContestMembership?> GetMembershipAsync(int contestId, int userId, CancellationToken cancellationToken = default);
}
