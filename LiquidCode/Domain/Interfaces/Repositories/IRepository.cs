namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Базовый интерфейс репозитория для общих операций CRUD
/// </summary>
/// <typeparam name="TEntity">Тип сущности, управляемой этим репозиторием</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Находит сущность по ее ID
    /// </summary>
    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все сущности
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет новую сущность
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет сущность
    /// </summary>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет все изменения, сделанные в базе данных
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
