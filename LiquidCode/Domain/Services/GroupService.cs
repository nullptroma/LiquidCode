using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
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

        var group = new DbGroup
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _groupRepository.CreateAsync(group, cancellationToken);
        await _groupRepository.UpsertMembershipAsync(group.Id, ownerId, GroupMembershipRole.Administrator, cancellationToken);

        var full = await _groupRepository.FindWithDetailsAsync(group.Id, cancellationToken);
        return full == null ? null : GroupResponse.FromEntity(full);
    }

    public async Task<GroupResponse?> UpdateAsync(int groupId, UpdateGroupRequest request, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, cancellationToken);
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

        var updated = await _groupRepository.FindWithDetailsAsync(group.Id, cancellationToken);
        return updated == null ? null : GroupResponse.FromEntity(updated);
    }

    public async Task<bool> DeleteAsync(int groupId, int requesterId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, cancellationToken);
        if (group == null)
            return false;

        if (!IsAdmin(group, requesterId))
            return false;

        await _groupRepository.SoftDeleteAsync(group, cancellationToken);
        return true;
    }

    public async Task<GroupResponse?> GetAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, cancellationToken);
        return group == null ? null : GroupResponse.FromEntity(group);
    }

    public async Task<GroupsPageResponse?> GetForUserAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        var (groups, hasNext) = await _groupRepository.GetForUserAsync(userId, pageSize, pageNumber, cancellationToken);
        return new GroupsPageResponse(hasNext, groups.Select(GroupResponse.FromEntity));
    }

    public async Task<bool> UpsertMemberAsync(int groupId, int requesterId, int targetUserId, GroupMembershipRole role, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, cancellationToken);
        if (group == null)
            return false;

        if (!IsAdmin(group, requesterId))
            return false;

        if (await _userRepository.FindByIdAsync(targetUserId, cancellationToken) == null)
        {
            _logger.LogWarning("User {UserId} not found while adding to group {GroupId}", targetUserId, groupId);
            return false;
        }

        await _groupRepository.UpsertMembershipAsync(groupId, targetUserId, role, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int groupId, int requesterId, int targetUserId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, cancellationToken);
        if (group == null)
            return false;

        if (!IsAdmin(group, requesterId))
            return false;

        await _groupRepository.RemoveMembershipAsync(groupId, targetUserId, cancellationToken);
        return true;
    }

    private static bool IsAdmin(DbGroup group, int userId)
    {
        var membership = group.Memberships.FirstOrDefault(m => m.UserId == userId);
        return membership != null && membership.Role.HasFlag(GroupMembershipRole.Administrator);
    }
}
