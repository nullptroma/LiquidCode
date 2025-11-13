using System;
using System.Linq;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Репозиторий сообщений чат группы
/// </summary>
public class GroupChatRepository : IGroupChatRepository
{
    private readonly LiquidDbContext _dbContext;

    public GroupChatRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DbGroupChatMessage?> FindByIdAsync(long messageId, CancellationToken cancellationToken = default) =>
        _dbContext.GroupChatMessages
            .Include(m => m.Author)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

    /// <summary>
    /// Получить сообщения из чата группы.
    /// Поведение зависит от наличия фильтров:
    /// - Если afterMessageId или afterCreatedAt указаны: возвращает новые сообщения после указанного ID/даты (для long polling)
    /// - Если оба параметра null: возвращает последние N сообщений (самые свежие)
    /// Результат всегда возвращается в хронологическом порядке (от старых к новым)
    /// </summary>
    public async Task<IReadOnlyList<DbGroupChatMessage>> GetMessagesAsync(
        int groupId,
        int limit,
        long? afterMessageId,
        DateTime? afterCreatedAt,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive", nameof(limit));

        var query = _dbContext.GroupChatMessages
            .Include(m => m.Author)
            .Where(m => m.GroupId == groupId);

        // Если указаны фильтры - получаем сообщения после них (для long polling)
        if (afterMessageId.HasValue || afterCreatedAt.HasValue)
        {
            if (afterMessageId.HasValue)
            {
                query = query.Where(m => m.Id > afterMessageId.Value);
            }

            if (afterCreatedAt.HasValue)
            {
                query = query.Where(m => m.CreatedAt > afterCreatedAt.Value);
            }

            // Сортируем по возрастанию ID (от старых к новым)
            return await query
                .OrderBy(m => m.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Если фильтры не указаны - получаем последние (самые свежие) сообщения
            // Сортируем по убыванию, берём limit, затем разворачиваем обратно
            var messages = await query
                .OrderByDescending(m => m.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);

            messages.Reverse();

            // Возвращаем в прямом порядке (от старых к новым)
            return messages;
        }
    }

    public async Task CreateAsync(DbGroupChatMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.GroupChatMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
