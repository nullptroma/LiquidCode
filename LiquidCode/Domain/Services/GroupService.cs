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

        var joinToken = await _groupRepository.RotateJoinTokenAsync(group.Id, ownerId, GroupDefaults.JoinLinkLifetime, cancellationToken);

        var full = await _groupRepository.FindWithDetailsAsync(group.Id, includeSoftDeleted: false, cancellationToken);
        if (full == null)
            return null;

        return GroupResponse.FromEntity(full, includePrivateDetails: true, joinToken);
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

        var updated = await _groupRepository.FindWithDetailsAsync(group.Id, includeSoftDeleted: false, cancellationToken);
        if (updated == null)
            return null;

        var joinToken = await EnsureJoinLinkAsync(groupId, requesterId, cancellationToken);
        return GroupResponse.FromEntity(updated, includePrivateDetails: true, joinToken);
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
        var joinToken = includePrivate
            ? await EnsureJoinLinkAsync(groupId, requesterId!.Value, cancellationToken)
            : await _groupRepository.GetActiveJoinTokenAsync(groupId, cancellationToken);

        return GroupResponse.FromEntity(group, includePrivate, joinToken);
    }

    public async Task<GroupsPageResponse?> GetForUserAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        var (groups, hasNext) = await _groupRepository.GetForUserAsync(userId, pageSize, pageNumber, cancellationToken);
        var responses = groups
            .Select(g => GroupResponse.FromEntity(g, includePrivateDetails: false, activeJoinToken: null))
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

    public async Task<GroupJoinLinkResponse?> GetJoinLinkAsync(int groupId, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        if (!IsAdmin(group, requesterId))
            return null;

        var token = await EnsureJoinLinkAsync(groupId, requesterId, cancellationToken);
        return new GroupJoinLinkResponse(token.Token, token.ExpiresAt);
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
            return GroupResponse.FromEntity(group, includePrivateDetails: false, activeJoinToken: null);

        await _groupRepository.UpsertMembershipAsync(
            group.Id,
            userId,
            GroupMembershipRole.Member,
            new GroupMembershipOptions(IsAutoJoined: true, JoinedAt: now),
            cancellationToken);

        joinToken.UsageCount += 1;
        joinToken.UpdatedAt = now;
        await _groupRepository.SaveChangesAsync(cancellationToken);

        var refreshed = await _groupRepository.FindWithDetailsAsync(group.Id, includeSoftDeleted: false, cancellationToken);
        if (refreshed == null)
            return null;

        var includePrivate = IsAdmin(refreshed, userId);
        var activeToken = includePrivate
            ? await EnsureJoinLinkAsync(refreshed.Id, userId, cancellationToken)
            : null;

        return GroupResponse.FromEntity(refreshed, includePrivate, activeToken);
    }

    private async Task<DbGroupJoinToken> EnsureJoinLinkAsync(int groupId, int rotatedById, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var active = await _groupRepository.GetActiveJoinTokenAsync(groupId, cancellationToken);
        if (active != null && active.ExpiresAt > now)
            return active;

        return await _groupRepository.RotateJoinTokenAsync(groupId, rotatedById, GroupDefaults.JoinLinkLifetime, cancellationToken);
    }

    private static bool IsAdmin(DbGroup group, int userId)
    {
        var membership = group.Memberships.FirstOrDefault(m => m.UserId == userId);
        return membership != null && membership.Role.HasFlag(GroupMembershipRole.Administrator);
    }
}
