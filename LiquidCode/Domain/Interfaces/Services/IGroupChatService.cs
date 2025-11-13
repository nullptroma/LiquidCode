using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис чата группы
/// </summary>
public interface IGroupChatService
{
    /// <summary>
    /// Отправить сообщение в чат группы
    /// </summary>
    Task<GroupChatMessageResponse?> SendMessageAsync(int groupId, int authorId, CreateGroupChatMessageRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить сообщения из чата группы.
    /// Если afterMessageId и afterCreatedAt не указаны - возвращает последние N сообщений.
    /// Если указаны - возвращает новые сообщения после указанного, с поддержкой long polling.
    /// </summary>
    /// <param name="groupId">ID группы</param>
    /// <param name="requesterId">ID пользователя, запрашивающего сообщения</param>
    /// <param name="limit">Максимальное количество сообщений</param>
    /// <param name="afterMessageId">ID сообщения, после которого получить новые (null = последние сообщения)</param>
    /// <param name="afterCreatedAt">Дата, после которой получить сообщения (null = последние сообщения)</param>
    /// <param name="timeoutSeconds">Таймаут ожидания новых сообщений (для long polling)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список сообщений или null, если группа не найдена/нет доступа</returns>
    Task<IReadOnlyList<GroupChatMessageResponse>?> GetMessagesAsync(int groupId, int requesterId, int limit, long? afterMessageId, DateTime? afterCreatedAt, int timeoutSeconds, CancellationToken cancellationToken = default);
}
