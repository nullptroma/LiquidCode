using System;
using System.Linq;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Репозиторий ленты групп
/// </summary>
public class GroupFeedRepository : IGroupFeedRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbGroupFeedPost> _crud;

    public GroupFeedRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _crud = new DbCrud<DbGroupFeedPost>(dbContext);
    }

    public Task<DbGroupFeedPost?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _crud.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbGroupFeedPost> Items, bool HasNextPage)> GetPageAsync(
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default) =>
        _crud.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbGroupFeedPost entity, CancellationToken cancellationToken = default) =>
        _crud.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbGroupFeedPost entity, CancellationToken cancellationToken = default) =>
        _crud.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbGroupFeedPost entity, CancellationToken cancellationToken = default) =>
        _crud.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbGroupFeedPost entity, CancellationToken cancellationToken = default) =>
        _crud.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _crud.SaveChangesAsync(cancellationToken);

    public Task<DbGroupFeedPost?> FindWithAuthorAsync(int groupId, int postId, CancellationToken cancellationToken = default) =>
        _dbContext.GroupFeedPosts
            .Include(p => p.Author)
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.Id == postId && p.GroupId == groupId && !p.IsDeleted, cancellationToken);

    public async Task<(IEnumerable<DbGroupFeedPost> Items, bool HasNextPage)> GetPageAsync(
        int groupId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var query = _dbContext.GroupFeedPosts
            .Include(p => p.Author)
            .Where(p => p.GroupId == groupId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var items = await query
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, hasNextPage);
    }
}
