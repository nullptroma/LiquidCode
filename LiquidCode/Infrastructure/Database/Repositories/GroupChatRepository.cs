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

        if (afterMessageId.HasValue)
        {
            query = query.Where(m => m.Id > afterMessageId.Value);
        }

        if (afterCreatedAt.HasValue)
        {
            query = query.Where(m => m.CreatedAt > afterCreatedAt.Value);
        }

        return await query
            .OrderBy(m => m.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(DbGroupChatMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.GroupChatMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
