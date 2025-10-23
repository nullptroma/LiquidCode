using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Интерфейс репозитория для операций с базой данных, связанных с отправками пользователей
/// </summary>
public interface ISubmitRepository : IRepository<DbUserSubmit>
{
    /// <summary>
    /// Получает отправки по пользователю
    /// </summary>
    Task<IEnumerable<DbUserSubmit>> GetSubmissionsByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает отправки по миссии
    /// </summary>
    Task<IEnumerable<DbUserSubmit>> GetSubmissionsByMissionAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает отправку со всеми связанными данными
    /// </summary>
    Task<DbUserSubmit?> GetSubmissionWithDetailsAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает решение для отправки
    /// </summary>
    Task<DbSolution?> GetSolutionAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет решение для отправки
    /// </summary>
    Task AddSolutionAsync(DbSolution solution, CancellationToken cancellationToken = default);
}
