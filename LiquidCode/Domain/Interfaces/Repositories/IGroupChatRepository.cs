using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для сообщений группового чата
/// </summary>
public interface IGroupChatRepository
{
    Task<DbGroupChatMessage?> FindByIdAsync(long messageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DbGroupChatMessage>> GetMessagesAsync(
        int groupId,
        int limit,
        long? afterMessageId,
        DateTime? afterCreatedAt,
        CancellationToken cancellationToken = default);

    Task CreateAsync(DbGroupChatMessage message, CancellationToken cancellationToken = default);
}
