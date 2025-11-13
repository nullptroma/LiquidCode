using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис чата группы
/// </summary>
public interface IGroupChatService
{
    Task<GroupChatMessageResponse?> SendMessageAsync(int groupId, int authorId, CreateGroupChatMessageRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GroupChatMessageResponse>?> GetMessagesAsync(int groupId, int requesterId, int limit, long? afterMessageId, DateTime? afterCreatedAt, CancellationToken cancellationToken = default);
}
