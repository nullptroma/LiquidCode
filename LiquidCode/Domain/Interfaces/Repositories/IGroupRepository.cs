using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для учебных групп
/// </summary>
public interface IGroupRepository : IRepository<DbGroup>
{
    Task<DbGroup?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbGroup> Items, bool HasNextPage)> GetForUserAsync(
        int userId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task UpsertMembershipAsync(int groupId, int userId, GroupMembershipRole role, CancellationToken cancellationToken = default);
    Task RemoveMembershipAsync(int groupId, int userId, CancellationToken cancellationToken = default);
}
