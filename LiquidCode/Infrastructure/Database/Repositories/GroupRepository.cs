using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Репозиторий для работы с группами
/// </summary>
public class GroupRepository : IGroupRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbGroup> _crud;

    public GroupRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _crud = new DbCrud<DbGroup>(dbContext);
    }

    public Task<DbGroup?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _crud.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbGroup> Items, bool HasNextPage)> GetPageAsync(
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default) =>
        _crud.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbGroup entity, CancellationToken cancellationToken = default) =>
        _crud.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbGroup entity, CancellationToken cancellationToken = default) =>
        _crud.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbGroup entity, CancellationToken cancellationToken = default) =>
        _crud.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbGroup entity, CancellationToken cancellationToken = default) =>
        _crud.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _crud.SaveChangesAsync(cancellationToken);

    public async Task<DbGroup?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Groups
            .Include(g => g.Memberships)
                .ThenInclude(m => m.User)
            .Include(g => g.Contests)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<(IEnumerable<DbGroup> Items, bool HasNextPage)> GetForUserAsync(
        int userId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var query = _dbContext.Groups
            .Include(g => g.Memberships)
                .ThenInclude(m => m.User)
            .Where(g => g.Memberships.Any(m => m.UserId == userId) && !g.IsDeleted)
            .OrderBy(g => g.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var items = await query
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, hasNextPage);
    }

    public async Task UpsertMembershipAsync(int groupId, int userId, GroupMembershipRole role, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);

        if (membership == null)
        {
            membership = new DbGroupMembership
            {
                GroupId = groupId,
                UserId = userId,
                Role = role
            };
            await _dbContext.GroupMemberships.AddAsync(membership, cancellationToken);
        }
        else
        {
            membership.Role = role;
            _dbContext.GroupMemberships.Update(membership);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMembershipAsync(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);

        if (membership != null)
        {
            _dbContext.GroupMemberships.Remove(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
