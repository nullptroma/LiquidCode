using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Репозиторий для работы с тегами
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbTag> _crud;

    public TagRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _crud = new DbCrud<DbTag>(dbContext);
    }

    public Task<DbTag?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _crud.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbTag> Items, bool HasNextPage)> GetPageAsync(
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default) =>
        _crud.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbTag entity, CancellationToken cancellationToken = default) =>
        _crud.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbTag entity, CancellationToken cancellationToken = default) =>
        _crud.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbTag entity, CancellationToken cancellationToken = default) =>
        _crud.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbTag entity, CancellationToken cancellationToken = default) =>
        _crud.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _crud.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<DbTag>> FindByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var normalized = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .ToArray();

        if (normalized.Length == 0)
            return Array.Empty<DbTag>();

        var lowered = normalized.Select(n => n.ToLowerInvariant()).ToArray();

        return await _dbContext.Tags
            .Where(t => lowered.Contains(t.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DbTag>> SearchAsync(string? query, int limit, CancellationToken cancellationToken = default)
    {
        var normalized = query?.Trim();

        var tags = _dbContext.Tags.AsQueryable();
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            var lowered = normalized.ToLowerInvariant();
            tags = tags.Where(t => t.Name.ToLower().Contains(lowered));
        }

        return await tags
            .OrderBy(t => t.Name)
            .Take(Math.Max(1, limit))
            .ToListAsync(cancellationToken);
    }
}
