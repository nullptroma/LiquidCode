using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Интерфейс репозитория для операций с базой данных, связанных с пользователями
/// </summary>
public interface IUserRepository : IRepository<DbUser>
{
    /// <summary>
    /// Находит пользователя по имени пользователя
    /// </summary>
    Task<DbUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, существует ли пользователь с данным именем пользователя
    /// </summary>
    Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает количество токенов обновления для пользователя
    /// </summary>
    Task<int> GetRefreshTokenCountAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает самый старый токен обновления для пользователя (для очистки)
    /// </summary>
    Task<DbRefreshToken?> GetOldestRefreshTokenAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет токен обновления для пользователя
    /// </summary>
    Task AddRefreshTokenAsync(DbRefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет токен обновления по строке токена
    /// </summary>
    Task RemoveRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Находит токен обновления по его строковому значению
    /// </summary>
    Task<DbRefreshToken?> FindRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default);
}
