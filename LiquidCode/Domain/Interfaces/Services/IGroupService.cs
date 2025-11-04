using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис управления группами
/// </summary>
public interface IGroupService
{
    Task<GroupResponse?> CreateAsync(CreateGroupRequest request, int ownerId, CancellationToken cancellationToken = default);
    Task<GroupResponse?> UpdateAsync(int groupId, UpdateGroupRequest request, int requesterId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int groupId, int requesterId, CancellationToken cancellationToken = default);
    Task<GroupResponse?> GetAsync(int groupId, int? requesterId, CancellationToken cancellationToken = default);
    Task<GroupsPageResponse?> GetForUserAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<bool> UpdateMemberRoleAsync(int groupId, int requesterId, int targetUserId, GroupMembershipRole role, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(int groupId, int requesterId, int targetUserId, CancellationToken cancellationToken = default);
    Task<GroupJoinLinkResponse?> RotateJoinLinkAsync(int groupId, int requesterId, CancellationToken cancellationToken = default);
    Task<GroupInvitationResponse?> CreateInvitationAsync(int groupId, int requesterId, CreateGroupInvitationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GroupInvitationResponse>> GetPendingInvitationsAsync(int groupId, int requesterId, CancellationToken cancellationToken = default);
    Task<bool> CancelInvitationAsync(int groupId, int requesterId, int invitationId, CancellationToken cancellationToken = default);
    Task<bool> RespondToInvitationAsync(string token, int userId, bool accept, CancellationToken cancellationToken = default);
    Task<GroupResponse?> JoinByTokenAsync(string token, int userId, CancellationToken cancellationToken = default);
}
