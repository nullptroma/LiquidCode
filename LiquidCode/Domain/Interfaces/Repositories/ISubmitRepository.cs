using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Интерфейс репозитория для операций с базой данных, связанных с отправками пользователей
/// </summary>
public interface ISubmitRepository : IRepository<DbUserSubmission>
{
    /// <summary>
    /// Получает отправки по пользователю
    /// </summary>
    Task<IEnumerable<DbUserSubmission>> GetSubmissionsByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает отправки по миссии
    /// </summary>
    Task<IEnumerable<DbUserSubmission>> GetSubmissionsByMissionAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает отправки пользователя в рамках конкретного контеста
    /// </summary>
    Task<IEnumerable<DbUserSubmission>> GetSubmissionsByUserAndContestAsync(int userId, int contestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает отправку со всеми связанными данными
    /// </summary>
    Task<DbUserSubmission?> GetSubmissionWithDetailsAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает решение
    /// </summary>
    Task<DbSolution?> GetSolutionAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает отправку по идентификатору решения с загруженной попыткой контеста
    /// </summary>
    Task<DbUserSubmission?> GetSubmissionBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет решение
    /// </summary>
    Task AddSolutionAsync(DbSolution solution, CancellationToken cancellationToken = default);
}
