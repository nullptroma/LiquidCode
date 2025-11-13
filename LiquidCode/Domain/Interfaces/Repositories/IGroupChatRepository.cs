using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для сообщений группового чата
/// </summary>
public interface IGroupChatRepository
{
    Task<DbGroupChatMessage?> FindByIdAsync(long messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сообщения из чата группы.
    /// Если afterMessageId и afterCreatedAt не указаны - возвращает последние N сообщений (самые свежие).
    /// Если указаны - возвращает новые сообщения после указанного ID/даты.
    /// Результат всегда в хронологическом порядке (от старых к новым).
    /// </summary>
    Task<IReadOnlyList<DbGroupChatMessage>> GetMessagesAsync(
        int groupId,
        int limit,
        long? afterMessageId,
        DateTime? afterCreatedAt,
        CancellationToken cancellationToken = default);

    Task CreateAsync(DbGroupChatMessage message, CancellationToken cancellationToken = default);
}
