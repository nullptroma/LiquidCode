using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для работы с тегами контента
/// </summary>
public interface ITagRepository : IRepository<DbTag>
{
    /// <summary>
    /// Находит теги по именам
/// </summary>
    Task<IReadOnlyList<DbTag>> FindByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает теги, удовлетворяющие поисковому запросу
    /// </summary>
    Task<IReadOnlyList<DbTag>> SearchAsync(string? query, int limit, CancellationToken cancellationToken = default);
}
