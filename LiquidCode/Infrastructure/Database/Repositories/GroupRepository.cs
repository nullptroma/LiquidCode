using System;
using System.Linq;
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

    public Task<DbGroup?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        FindWithDetailsAsync(id, includeSoftDeleted: false, cancellationToken);

    public async Task<DbGroup?> FindWithDetailsAsync(int id, bool includeSoftDeleted, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Groups
            .Include(g => g.Memberships)
                .ThenInclude(m => m.User)
            .Include(g => g.Contests)
            .Include(g => g.JoinTokens)
            .AsQueryable();

        if (!includeSoftDeleted)
        {
            query = query.Where(g => !g.IsDeleted);
        }

        return await query.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

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

    public Task<DbGroupMembership?> GetMembershipAsync(int groupId, int userId, CancellationToken cancellationToken = default) =>
        _dbContext.GroupMemberships
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);

    public async Task UpsertMembershipAsync(int groupId, int userId, GroupMembershipRole role, GroupMembershipOptions? options, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);

        if (membership == null)
        {
            membership = new DbGroupMembership
            {
                GroupId = groupId,
                UserId = userId,
                Role = role,
                JoinedAt = options?.JoinedAt ?? DateTime.UtcNow,
                InvitedById = options?.InvitedById,
                InvitationId = options?.InvitationId,
                IsAutoJoined = options?.IsAutoJoined ?? false
            };
            await _dbContext.GroupMemberships.AddAsync(membership, cancellationToken);
        }
        else
        {
            membership.Role = role;
            membership.InvitedById = options?.InvitedById ?? membership.InvitedById;
            membership.InvitationId = options?.InvitationId ?? membership.InvitationId;
            membership.IsAutoJoined = options?.IsAutoJoined ?? membership.IsAutoJoined;
            if (options?.JoinedAt != null)
            {
                membership.JoinedAt = options.JoinedAt.Value;
            }
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

    public Task<DbGroupJoinToken?> GetActiveJoinTokenAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return _dbContext.GroupJoinTokens
            .Where(t => t.GroupId == groupId && t.RevokedAt == null && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<DbGroupJoinToken?> GetJoinTokenByValueAsync(string token, CancellationToken cancellationToken = default) =>
        _dbContext.GroupJoinTokens
            .Include(t => t.Group)
                .ThenInclude(g => g.Memberships)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task<DbGroupJoinToken> RotateJoinTokenAsync(int groupId, int createdById, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var activeTokens = await _dbContext.GroupJoinTokens
            .Where(t => t.GroupId == groupId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
            token.UpdatedAt = now;
        }

        var joinToken = new DbGroupJoinToken
        {
            GroupId = groupId,
            CreatedById = createdById,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = now.Add(ttl),
            LastRefreshedAt = now,
            UsageCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _dbContext.GroupJoinTokens.AddAsync(joinToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return joinToken;
    }
}
