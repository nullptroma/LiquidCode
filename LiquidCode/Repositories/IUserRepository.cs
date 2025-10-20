using LiquidCode.Models.Database;

namespace LiquidCode.Repositories;

/// <summary>
/// Repository interface for user-related database operations
/// </summary>
public interface IUserRepository : IRepository<DbUser>
{
    /// <summary>
    /// Finds a user by their username
    /// </summary>
    Task<DbUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given username exists
    /// </summary>
    Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of refresh tokens for a user
    /// </summary>
    Task<int> GetRefreshTokenCountAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the oldest refresh token for a user (for cleanup)
    /// </summary>
    Task<DbRefreshToken?> GetOldestRefreshTokenAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a refresh token for a user
    /// </summary>
    Task AddRefreshTokenAsync(DbRefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a refresh token by token string
    /// </summary>
    Task RemoveRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a refresh token by its string value
    /// </summary>
    Task<DbRefreshToken?> FindRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default);
}
