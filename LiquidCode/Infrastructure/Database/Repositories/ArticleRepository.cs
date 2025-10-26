using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Репозиторий для работы со статьями
/// </summary>
public class ArticleRepository : IArticleRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbArticle> _crud;

    public ArticleRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _crud = new DbCrud<DbArticle>(dbContext);
    }

    public Task<DbArticle?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _crud.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbArticle> Items, bool HasNextPage)> GetPageAsync(
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default) =>
        _crud.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbArticle entity, CancellationToken cancellationToken = default) =>
        _crud.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbArticle entity, CancellationToken cancellationToken = default) =>
        _crud.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbArticle entity, CancellationToken cancellationToken = default) =>
        _crud.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbArticle entity, CancellationToken cancellationToken = default) =>
        _crud.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _crud.SaveChangesAsync(cancellationToken);

    public async Task<DbArticle?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbContext.Articles
            .Include(a => a.Author)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Include(a => a.ContestEntries)
                .ThenInclude(ca => ca.Contest)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IEnumerable<DbArticle> Items, bool HasNextPage)> GetFilteredPageAsync(
        int pageSize,
        int pageNumber,
        IEnumerable<int>? tagIds,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0 || pageNumber < 0)
            throw new ArgumentException("Page size must be positive, page number must be non-negative");

        var query = _dbContext.Articles
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => !a.IsDeleted);

        if (tagIds != null)
        {
            var tagArray = tagIds.ToArray();
            if (tagArray.Length > 0)
            {
                query = query.Where(a => a.ArticleTags.Any(at => tagArray.Contains(at.TagId)));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var hasNextPage = totalCount > pageSize * (pageNumber + 1);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(pageSize * pageNumber)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, hasNextPage);
    }

    public async Task SyncTagsAsync(DbArticle article, IEnumerable<int> tagIds, CancellationToken cancellationToken = default)
    {
        var targetIds = tagIds?.ToHashSet() ?? new HashSet<int>();

        var existing = await _dbContext.ArticleTags
            .Where(at => at.ArticleId == article.Id)
            .ToListAsync(cancellationToken);

        var toRemove = existing.Where(e => !targetIds.Contains(e.TagId)).ToList();
        if (toRemove.Count > 0)
        {
            _dbContext.ArticleTags.RemoveRange(toRemove);
        }

        var existingIds = existing.Select(e => e.TagId).ToHashSet();
        var toAdd = targetIds.Except(existingIds)
            .Select(tagId => new DbArticleTag
            {
                ArticleId = article.Id,
                TagId = tagId
            }).ToList();

        if (toAdd.Count > 0)
        {
            await _dbContext.ArticleTags.AddRangeAsync(toAdd, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
