using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для учебных групп
/// </summary>
public interface IGroupRepository : IRepository<DbGroup>
{
    Task<DbGroup?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<DbGroup?> FindWithDetailsAsync(int id, bool includeSoftDeleted, CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbGroup> Items, bool HasNextPage)> GetForUserAsync(
        int userId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);

    Task<DbGroupMembership?> GetMembershipAsync(int groupId, int userId, CancellationToken cancellationToken = default);
    Task UpsertMembershipAsync(int groupId, int userId, GroupMembershipRole role, GroupMembershipOptions? options, CancellationToken cancellationToken = default);
    Task RemoveMembershipAsync(int groupId, int userId, CancellationToken cancellationToken = default);
    Task<DbGroupJoinToken?> GetActiveJoinTokenAsync(int groupId, CancellationToken cancellationToken = default);
    Task<DbGroupJoinToken?> GetJoinTokenByValueAsync(string token, CancellationToken cancellationToken = default);
    Task<DbGroupJoinToken> RotateJoinTokenAsync(int groupId, int createdById, TimeSpan ttl, CancellationToken cancellationToken = default);
}

public record GroupMembershipOptions(
    int? InvitedById = null,
    int? InvitationId = null,
    bool IsAutoJoined = false,
    DateTime? JoinedAt = null
);
