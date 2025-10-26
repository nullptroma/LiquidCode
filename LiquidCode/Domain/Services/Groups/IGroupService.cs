using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Services.Groups;

/// <summary>
/// Сервис управления группами
/// </summary>
public interface IGroupService
{
    Task<GroupResponse?> CreateAsync(CreateGroupRequest request, int ownerId, CancellationToken cancellationToken = default);
    Task<GroupResponse?> UpdateAsync(int groupId, UpdateGroupRequest request, int requesterId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int groupId, int requesterId, CancellationToken cancellationToken = default);
    Task<GroupResponse?> GetAsync(int groupId, CancellationToken cancellationToken = default);
    Task<GroupsPageResponse?> GetForUserAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<bool> UpsertMemberAsync(int groupId, int requesterId, int targetUserId, GroupMembershipRole role, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(int groupId, int requesterId, int targetUserId, CancellationToken cancellationToken = default);
}
