using System;
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
/// Реализация сервиса ленты группы
/// </summary>
public class GroupFeedService : IGroupFeedService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupFeedRepository _groupFeedRepository;
    private readonly ILogger<GroupFeedService> _logger;

    public GroupFeedService(
        IGroupRepository groupRepository,
        IGroupFeedRepository groupFeedRepository,
        ILogger<GroupFeedService> logger)
    {
        _groupRepository = groupRepository;
        _groupFeedRepository = groupFeedRepository;
        _logger = logger;
    }

    public async Task<GroupFeedPostResponse?> CreateAsync(int groupId, int authorId, CreateGroupFeedPostRequest request, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(groupId, authorId, requireAdmin: true, cancellationToken);
        if (membership == null)
        {
            _logger.LogWarning("User {UserId} attempted to create feed post for group {GroupId} without permissions", authorId, groupId);
            return null;
        }

        var content = request.Content.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Attempt to create empty feed post in group {GroupId} by user {UserId}", groupId, authorId);
            return null;
        }

        var post = new DbGroupFeedPost
        {
            GroupId = groupId,
            AuthorId = authorId,
            Author = membership.User,
            Name = string.IsNullOrWhiteSpace(request.Name) ? GroupCommunicationDefaults.DefaultFeedPostTitle : request.Name.Trim(),
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _groupFeedRepository.CreateAsync(post, cancellationToken);

        var stored = await _groupFeedRepository.FindWithAuthorAsync(groupId, post.Id, cancellationToken);
        return stored == null ? null : GroupFeedPostResponse.FromEntity(stored);
    }

    public async Task<GroupFeedPostResponse?> UpdateAsync(int groupId, int postId, int requesterId, UpdateGroupFeedPostRequest request, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(groupId, requesterId, requireAdmin: false, cancellationToken);
        if (membership == null)
            return null;

        var post = await _groupFeedRepository.FindWithAuthorAsync(groupId, postId, cancellationToken);
        if (post == null)
            return null;

        if (post.AuthorId != requesterId)
        {
            _logger.LogWarning("User {UserId} attempted to edit feed post {PostId} in group {GroupId} without authorship", requesterId, postId, groupId);
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            post.Name = request.Name.Trim();

        if (request.Content != null)
        {
            var trimmed = request.Content.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                _logger.LogWarning("User {UserId} attempted to set empty content for feed post {PostId}", requesterId, postId);
                return null;
            }

            post.Content = trimmed;
        }

        post.UpdatedAt = DateTime.UtcNow;

        await _groupFeedRepository.UpdateAsync(post, cancellationToken);

        var stored = await _groupFeedRepository.FindWithAuthorAsync(groupId, post.Id, cancellationToken);
        return stored == null ? null : GroupFeedPostResponse.FromEntity(stored);
    }

    public async Task<bool> DeleteAsync(int groupId, int postId, int requesterId, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(groupId, requesterId, requireAdmin: false, cancellationToken);
        if (membership == null)
            return false;

        var post = await _groupFeedRepository.FindWithAuthorAsync(groupId, postId, cancellationToken);
        if (post == null)
            return false;

        if (post.AuthorId != requesterId && !IsAdmin(membership.Role))
        {
            _logger.LogWarning("User {UserId} attempted to delete feed post {PostId} in group {GroupId} without permissions", requesterId, postId, groupId);
            return false;
        }

        await _groupFeedRepository.SoftDeleteAsync(post, cancellationToken);
        return true;
    }

    public async Task<GroupFeedPostResponse?> GetAsync(int groupId, int postId, int requesterId, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(groupId, requesterId, requireAdmin: false, cancellationToken);
        if (membership == null)
            return null;

        var post = await _groupFeedRepository.FindWithAuthorAsync(groupId, postId, cancellationToken);
        return post == null ? null : GroupFeedPostResponse.FromEntity(post);
    }

    public async Task<GroupFeedPageResponse?> GetPageAsync(int groupId, int requesterId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            return null;

        if (pageSize > GroupCommunicationDefaults.FeedPageSizeLimit)
            pageSize = GroupCommunicationDefaults.FeedPageSizeLimit;

        var membership = await GetMembershipAsync(groupId, requesterId, requireAdmin: false, cancellationToken);
        if (membership == null)
            return null;

        var (items, hasNext) = await _groupFeedRepository.GetPageAsync(groupId, pageSize, pageNumber, cancellationToken);
        var responses = items.Select(GroupFeedPostResponse.FromEntity).ToList();
        return new GroupFeedPageResponse(hasNext, responses);
    }

    private async Task<DbGroupMembership?> GetMembershipAsync(int groupId, int userId, bool requireAdmin, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        var membership = group.Memberships.FirstOrDefault(m => m.UserId == userId);
        if (membership == null)
            return null;

        if (requireAdmin && !IsAdmin(membership.Role))
            return null;

        return membership;
    }

    private static bool IsAdmin(GroupMembershipRole role) =>
        role.HasFlag(GroupMembershipRole.Administrator) || role.HasFlag(GroupMembershipRole.Creator);
}
