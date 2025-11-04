using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Groups;

/// <summary>
/// Реализация сервиса групп
/// </summary>
public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IGroupRepository groupRepository, IUserRepository userRepository, ILogger<GroupService> logger)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<GroupResponse?> CreateAsync(CreateGroupRequest request, int ownerId, CancellationToken cancellationToken = default)
    {
        var owner = await _userRepository.FindByIdAsync(ownerId, cancellationToken);
        if (owner == null)
            return null;

        var now = DateTime.UtcNow;

        var group = new DbGroup
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _groupRepository.CreateAsync(group, cancellationToken);

        await _groupRepository.UpsertMembershipAsync(
            group.Id,
            ownerId,
            GroupMembershipRole.Creator | GroupMembershipRole.Administrator,
            new GroupMembershipOptions(JoinedAt: now),
            cancellationToken);

        var joinToken = await _groupRepository.RotateJoinTokenAsync(group.Id, ownerId, GroupInvitationDefaults.JoinTokenTtl, cancellationToken);

        var full = await _groupRepository.FindWithDetailsAsync(group.Id, includeSoftDeleted: false, cancellationToken);
        if (full == null)
            return null;

        return GroupResponse.FromEntity(full, includePrivateDetails: true, joinToken, invitations: Array.Empty<DbGroupInvitation>());
    }

    public async Task<GroupResponse?> UpdateAsync(int groupId, UpdateGroupRequest request, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        if (!IsAdmin(group, requesterId))
            return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            group.Name = request.Name.Trim();

        if (request.Description != null)
            group.Description = request.Description.Trim();

        group.UpdatedAt = DateTime.UtcNow;

        await _groupRepository.UpdateAsync(group, cancellationToken);

        var joinToken = await _groupRepository.GetActiveJoinTokenAsync(groupId, cancellationToken);
        var invitations = await _groupRepository.GetActiveInvitationsAsync(groupId, cancellationToken);

        var updated = await _groupRepository.FindWithDetailsAsync(group.Id, includeSoftDeleted: false, cancellationToken);
        return updated == null
            ? null
            : GroupResponse.FromEntity(updated, includePrivateDetails: true, joinToken, invitations);
    }

    public async Task<bool> DeleteAsync(int groupId, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return false;

        if (!IsAdmin(group, requesterId))
            return false;

        await _groupRepository.SoftDeleteAsync(group, cancellationToken);
        return true;
    }

    public async Task<GroupResponse?> GetAsync(int groupId, int? requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        var includePrivate = requesterId.HasValue && IsAdmin(group, requesterId.Value);
        var joinToken = includePrivate ? await _groupRepository.GetActiveJoinTokenAsync(groupId, cancellationToken) : null;
        var invitations = includePrivate ? await _groupRepository.GetActiveInvitationsAsync(groupId, cancellationToken) : Array.Empty<DbGroupInvitation>();

        return GroupResponse.FromEntity(group, includePrivate, joinToken, invitations);
    }

    public async Task<GroupsPageResponse?> GetForUserAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        var (groups, hasNext) = await _groupRepository.GetForUserAsync(userId, pageSize, pageNumber, cancellationToken);
        var responses = groups
            .Select(g => GroupResponse.FromEntity(g, includePrivateDetails: false, activeJoinToken: null, invitations: null))
            .ToList();

        return new GroupsPageResponse(hasNext, responses);
    }

    public async Task<bool> UpdateMemberRoleAsync(int groupId, int requesterId, int targetUserId, GroupMembershipRole role, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return false;

        if (!IsAdmin(group, requesterId))
            return false;

        var membership = group.Memberships.FirstOrDefault(m => m.UserId == targetUserId);
        if (membership == null)
            return false;

        if (membership.Role.HasFlag(GroupMembershipRole.Creator) && !role.HasFlag(GroupMembershipRole.Creator))
        {
            _logger.LogWarning("Attempt to remove creator role from user {UserId} in group {GroupId}", targetUserId, groupId);
            return false;
        }

        await _groupRepository.UpsertMembershipAsync(
            groupId,
            targetUserId,
            role,
            new GroupMembershipOptions(
                InvitedById: membership.InvitedById,
                InvitationId: membership.InvitationId,
                IsAutoJoined: membership.IsAutoJoined,
                JoinedAt: membership.JoinedAt),
            cancellationToken);

        return true;
    }

    public async Task<bool> RemoveMemberAsync(int groupId, int requesterId, int targetUserId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return false;

        if (!IsAdmin(group, requesterId))
            return false;

        var membership = group.Memberships.FirstOrDefault(m => m.UserId == targetUserId);
        if (membership == null)
            return false;

        if (membership.Role.HasFlag(GroupMembershipRole.Creator))
        {
            _logger.LogWarning("Attempt to remove creator {UserId} from group {GroupId}", targetUserId, groupId);
            return false;
        }

        await _groupRepository.RemoveMembershipAsync(groupId, targetUserId, cancellationToken);
        return true;
    }

    public async Task<GroupJoinLinkResponse?> RotateJoinLinkAsync(int groupId, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        if (!IsAdmin(group, requesterId))
            return null;

        var token = await _groupRepository.RotateJoinTokenAsync(groupId, requesterId, GroupInvitationDefaults.JoinTokenTtl, cancellationToken);
        return new GroupJoinLinkResponse(token.Token, token.ExpiresAt);
    }

    public async Task<GroupInvitationResponse?> CreateInvitationAsync(int groupId, int requesterId, CreateGroupInvitationRequest request, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        if (!IsAdmin(group, requesterId))
            return null;

        var invitee = await ResolveInviteeAsync(request, cancellationToken);
        if (invitee == null)
        {
            _logger.LogWarning("Invitee not found for target {Target} via {Channel}", request.Target, request.DeliveryChannel);
            return null;
        }

        if (group.Memberships.Any(m => m.UserId == invitee.Id))
        {
            _logger.LogInformation("User {UserId} already member of group {GroupId}", invitee.Id, groupId);
            return null;
        }

        var existing = await _groupRepository.GetActiveInvitationsAsync(groupId, cancellationToken);
        if (existing.Any(i => i.InviteeId == invitee.Id))
        {
            _logger.LogInformation("Active invitation already exists for user {UserId} in group {GroupId}", invitee.Id, groupId);
            return null;
        }

        var now = DateTime.UtcNow;
        var invitation = new DbGroupInvitation
        {
            GroupId = groupId,
            InviterId = requesterId,
            InviteeId = invitee.Id,
            DeliveryChannel = request.DeliveryChannel,
            ExpiresAt = now.Add(GroupInvitationDefaults.InvitationTtl),
            Token = Guid.NewGuid().ToString("N"),
            Status = GroupInvitationStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        var saved = await _groupRepository.AddInvitationAsync(invitation, cancellationToken);

        return new GroupInvitationResponse(
            saved.Id,
            saved.InviteeId,
            invitee.Username,
            saved.Status,
            saved.ExpiresAt,
            saved.CreatedAt);
    }

    public async Task<IReadOnlyList<GroupInvitationResponse>> GetPendingInvitationsAsync(int groupId, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null || !IsAdmin(group, requesterId))
            return Array.Empty<GroupInvitationResponse>();

        var invitations = await _groupRepository.GetActiveInvitationsAsync(groupId, cancellationToken);
        return invitations
            .Select(i => new GroupInvitationResponse(i.Id, i.InviteeId, i.Invitee.Username, i.Status, i.ExpiresAt, i.CreatedAt))
            .ToList();
    }

    public async Task<bool> CancelInvitationAsync(int groupId, int requesterId, int invitationId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null || !IsAdmin(group, requesterId))
            return false;

        var invitation = await _groupRepository.GetInvitationByIdAsync(groupId, invitationId, cancellationToken);
        if (invitation == null)
            return false;

        if (invitation.Status != GroupInvitationStatus.Pending)
            return false;

        invitation.Status = GroupInvitationStatus.Revoked;
        invitation.RevokedAt = DateTime.UtcNow;
        invitation.UpdatedAt = DateTime.UtcNow;

        await _groupRepository.SaveInvitationAsync(invitation, cancellationToken);
        return true;
    }

    public async Task<bool> RespondToInvitationAsync(string token, int userId, bool accept, CancellationToken cancellationToken = default)
    {
        var invitation = await _groupRepository.GetInvitationByTokenAsync(token, cancellationToken);
        if (invitation == null || invitation.InviteeId != userId)
            return false;

        if (invitation.Status != GroupInvitationStatus.Pending)
            return false;

        var now = DateTime.UtcNow;
        if (invitation.ExpiresAt <= now || invitation.RevokedAt != null)
        {
            invitation.Status = GroupInvitationStatus.Expired;
            invitation.UpdatedAt = now;
            await _groupRepository.SaveInvitationAsync(invitation, cancellationToken);
            return false;
        }

        if (invitation.Group.IsDeleted)
        {
            invitation.Status = GroupInvitationStatus.Revoked;
            invitation.UpdatedAt = now;
            await _groupRepository.SaveInvitationAsync(invitation, cancellationToken);
            return false;
        }

        if (accept)
        {
            invitation.Status = GroupInvitationStatus.Accepted;
            invitation.AcceptedAt = now;

            await _groupRepository.UpsertMembershipAsync(
                invitation.GroupId,
                userId,
                GroupMembershipRole.Member,
                new GroupMembershipOptions(
                    InvitedById: invitation.InviterId,
                    InvitationId: invitation.Id,
                    JoinedAt: now),
                cancellationToken);
        }
        else
        {
            invitation.Status = GroupInvitationStatus.Declined;
            invitation.DeclinedAt = now;
        }

        invitation.UpdatedAt = now;
        await _groupRepository.SaveInvitationAsync(invitation, cancellationToken);

        return true;
    }

    public async Task<GroupResponse?> JoinByTokenAsync(string token, int userId, CancellationToken cancellationToken = default)
    {
        var joinToken = await _groupRepository.GetJoinTokenByValueAsync(token, cancellationToken);
        if (joinToken == null)
            return null;

        var now = DateTime.UtcNow;
        if (joinToken.RevokedAt != null || joinToken.ExpiresAt <= now)
            return null;

        var group = await _groupRepository.FindWithDetailsAsync(joinToken.GroupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        if (group.Memberships.Any(m => m.UserId == userId))
            return GroupResponse.FromEntity(group, includePrivateDetails: false, activeJoinToken: null, invitations: null);

        await _groupRepository.UpsertMembershipAsync(
            group.Id,
            userId,
            GroupMembershipRole.Member,
            new GroupMembershipOptions(IsAutoJoined: true, JoinedAt: now),
            cancellationToken);

        joinToken.UsageCount += 1;
        joinToken.UpdatedAt = now;
        await _groupRepository.SaveChangesAsync(cancellationToken);

        var includePrivate = IsAdmin(group, userId);
        var activeToken = includePrivate ? await _groupRepository.GetActiveJoinTokenAsync(group.Id, cancellationToken) : null;
        var invitations = includePrivate ? await _groupRepository.GetActiveInvitationsAsync(group.Id, cancellationToken) : Array.Empty<DbGroupInvitation>();

        var refreshed = await _groupRepository.FindWithDetailsAsync(group.Id, includeSoftDeleted: false, cancellationToken);
        return refreshed == null
            ? null
            : GroupResponse.FromEntity(refreshed, includePrivate, activeToken, invitations);
    }

    private async Task<DbUser?> ResolveInviteeAsync(CreateGroupInvitationRequest request, CancellationToken cancellationToken)
    {
        switch (request.DeliveryChannel)
        {
            case GroupInvitationDeliveryChannel.Username:
                return await _userRepository.FindByUsernameAsync(request.Target, cancellationToken);
            case GroupInvitationDeliveryChannel.Email:
                return await _userRepository.FindByEmailAsync(request.Target, cancellationToken);
            default:
                return null;
        }
    }

    private static bool IsAdmin(DbGroup group, int userId)
    {
        var membership = group.Memberships.FirstOrDefault(m => m.UserId == userId);
        return membership != null && membership.Role.HasFlag(GroupMembershipRole.Administrator);
    }
}
