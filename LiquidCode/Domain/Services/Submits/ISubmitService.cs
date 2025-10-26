using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Services.Submits;

/// <summary>
/// Интерфейс сервиса для операций, связанных с отправками пользователей
/// </summary>
public interface ISubmitService
{
    /// <summary>
    /// Отправляет решение для миссии
    /// </summary>
    /// <param name="missionId">ID миссии</param>
    /// <param name="userId">ID пользователя, отправляющего решение</param>
    /// <param name="sourceCode">Содержимое исходного кода</param>
    /// <param name="language">Язык программирования</param>
    /// <param name="languageVersion">Версия языка программирования</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданное решение или null, если отправка не удалась</returns>
    Task<DbSolution?> SubmitSolutionAsync(
        int missionId, int userId, string sourceCode, string language, string languageVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает конкретную отправку
    /// </summary>
    /// <param name="submissionId">ID отправки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Отправка с связанными данными или null, если не найдена</returns>
    Task<DbUserSubmission?> GetSubmissionAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все отправки пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список отправок</returns>
    Task<IEnumerable<DbUserSubmission>> GetUserSubmissionsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все отправки для миссии
    /// </summary>
    /// <param name="missionId">ID миссии</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список отправок</returns>
    Task<IEnumerable<DbUserSubmission>> GetMissionSubmissionsAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет статус решения
    /// </summary>
    /// <param name="solutionId">ID решения</param>
    /// <param name="status">Новый статус</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленное решение или null, если не найдено</returns>
    Task<DbSolution?> UpdateSolutionStatusAsync(int solutionId, string status, CancellationToken cancellationToken = default);

}
