using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для статей
/// </summary>
public interface IArticleRepository : IRepository<DbArticle>
{
    /// <summary>
    /// Возвращает статью с тегами и связями с контестами
    /// </summary>
    Task<DbArticle?> FindWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает страницу статей с возможной фильтрацией по тегам
    /// </summary>
    Task<(IEnumerable<DbArticle> Items, bool HasNextPage)> GetFilteredPageAsync(
        int pageSize,
        int pageNumber,
        IEnumerable<int>? tagIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает статьи, созданные указанным автором
    /// </summary>
    Task<IReadOnlyList<DbArticle>> GetByAuthorAsync(int authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет связи статьи с тегами
    /// </summary>
    Task SyncTagsAsync(DbArticle article, IEnumerable<int> tagIds, CancellationToken cancellationToken = default);
}
