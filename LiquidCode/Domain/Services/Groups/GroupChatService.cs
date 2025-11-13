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
/// Реализация сервиса чата групп
/// </summary>
public class GroupChatService : IGroupChatService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupChatRepository _groupChatRepository;
    private readonly ILogger<GroupChatService> _logger;

    public GroupChatService(
        IGroupRepository groupRepository,
        IGroupChatRepository groupChatRepository,
        ILogger<GroupChatService> logger)
    {
        _groupRepository = groupRepository;
        _groupChatRepository = groupChatRepository;
        _logger = logger;
    }

    public async Task<GroupChatMessageResponse?> SendMessageAsync(int groupId, int authorId, CreateGroupChatMessageRequest request, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(groupId, authorId, cancellationToken);
        if (membership == null)
        {
            _logger.LogWarning("User {UserId} attempted to send chat message to group {GroupId} without membership", authorId, groupId);
            return null;
        }

        var content = request.Content.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("User {UserId} attempted to send empty chat message to group {GroupId}", authorId, groupId);
            return null;
        }

        if (content.Length > GroupCommunicationDefaults.ChatMessageMaxLength)
        {
            _logger.LogWarning("Chat message too long in group {GroupId} by user {UserId}", groupId, authorId);
            return null;
        }

        var message = new DbGroupChatMessage
        {
            GroupId = groupId,
            AuthorId = authorId,
            Author = membership.User,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _groupChatRepository.CreateAsync(message, cancellationToken);

        var stored = await _groupChatRepository.FindByIdAsync(message.Id, cancellationToken);
        return stored == null ? null : GroupChatMessageResponse.FromEntity(stored);
    }

    public async Task<IReadOnlyList<GroupChatMessageResponse>?> GetMessagesAsync(int groupId, int requesterId, int limit, long? afterMessageId, DateTime? afterCreatedAt, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
            return null;

        limit = Math.Min(limit, GroupCommunicationDefaults.ChatPullLimit);

        var membership = await GetMembershipAsync(groupId, requesterId, cancellationToken);
        if (membership == null)
            return null;

        // Long polling: если запрашивают новые сообщения, ждём их появления
        if (timeoutSeconds > 0 && (afterMessageId.HasValue || afterCreatedAt.HasValue))
        {
            var pollInterval = TimeSpan.FromMilliseconds(500); // Интервал опроса БД
            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
            {
                var messages = await _groupChatRepository.GetMessagesAsync(groupId, limit, afterMessageId, afterCreatedAt, cancellationToken);
                
                if (messages.Count > 0)
                {
                    return messages.Select(GroupChatMessageResponse.FromEntity).ToList();
                }

                // Ждём перед следующей попыткой
                try
                {
                    await Task.Delay(pollInterval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        // Обычный запрос или истёк таймаут long polling
        var finalMessages = await _groupChatRepository.GetMessagesAsync(groupId, limit, afterMessageId, afterCreatedAt, cancellationToken);
        return finalMessages.Select(GroupChatMessageResponse.FromEntity).ToList();
    }

    private async Task<DbGroupMembership?> GetMembershipAsync(int groupId, int userId, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, includeSoftDeleted: false, cancellationToken);
        if (group == null)
            return null;

        return group.Memberships.FirstOrDefault(m => m.UserId == userId);
    }
}
