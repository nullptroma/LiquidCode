namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Base repository interface for common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this repository</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Finds an entity by its ID
    /// </summary>
    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity
    /// </summary>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes made to the database
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
