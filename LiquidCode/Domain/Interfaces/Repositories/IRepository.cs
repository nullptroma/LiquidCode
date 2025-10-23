using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Базовый интерфейс репозитория для общих операций CRUD
/// </summary>
/// <typeparam name="TEntity">Тип сущности, управляемой этим репозиторием</typeparam>
public interface IRepository<TEntity> where TEntity : class, ISoftDeletable
{
    /// <summary>
    /// Находит сущность по ее ID
    /// </summary>
    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все сущности с пагинацией
    /// </summary>
    /// <param name="pageSize">Количество элементов на странице</param>
    /// <param name="pageNumber">Номер страницы (начиная с 0)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Кортеж (сущности, естьСледующаяСтраница)</returns>
    Task<(IEnumerable<TEntity> Items, bool HasNextPage)> GetPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет новую сущность
    /// </summary>
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет сущность
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Мягко удаляет сущность
    /// </summary>
    Task SoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет все изменения, сделанные в базе данных
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
