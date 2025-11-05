using System.Collections.Generic;
using LiquidCode.Api.Submits.Dto;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Services;

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
    /// <param name="contestId">Идентификатор контеста, если отправка выполняется в его рамках</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданное решение или null, если отправка не удалась</returns>
    Task<DbSolution?> SubmitSolutionAsync(
        int missionId,
        int userId,
        string sourceCode,
        string language,
        string languageVersion,
        int? contestId,
        CancellationToken cancellationToken = default);

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
    /// Получает отправки пользователя в рамках конкретного контеста
    /// </summary>
    Task<ContestSubmissionsResult> GetUserContestSubmissionsAsync(int userId, int contestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправляет решение в тестирующий модуль.
    /// </summary>
    /// <param name="solution">Решение, подготовленное к тестированию.</param>
    /// <param name="callbackUrl">URL обратного вызова.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<SubmitDispatchResult> DispatchSolutionAsync(DbSolution solution, string callbackUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Применяет обновление статуса решения от тестирующего модуля
    /// </summary>
    /// <param name="solutionId">Идентификатор решения</param>
    /// <param name="callbackToken">Одноразовый токен обратного вызова</param>
    /// <param name="state">Новое состояние выполнения</param>
    /// <param name="errorCode">Информация об ошибке выполнения</param>
    /// <param name="message">Сообщение от тестирующего модуля</param>
    /// <param name="currentTest">Номер текущего теста</param>
    /// <param name="amountOfTests">Общее количество тестов</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат применения обновления</returns>
    Task<TesterCallbackUpdateResult> UpdateTesterStatusAsync(
        int solutionId,
        string callbackToken,
        TesterState state,
        TesterErrorCode errorCode,
        string? message,
        int currentTest,
        int amountOfTests,
        CancellationToken cancellationToken = default);

}

/// <summary>
/// Статус запроса отправок пользователя в рамках контеста
/// </summary>
public enum ContestSubmissionQueryStatus
{
    Success,
    ContestNotFound,
    AccessDenied,
    Error
}

/// <summary>
/// Результат запроса отправок пользователя в контесте
/// </summary>
public readonly record struct ContestSubmissionsResult(
    ContestSubmissionQueryStatus Status,
    IEnumerable<DbUserSubmission> Submissions);

/// <summary>
/// Возможный исход применения обратного вызова тестирующего модуля
/// </summary>
public enum TesterCallbackUpdateStatus
{
    Success,
    NotFound,
    TokenMismatch,
    Error
}

/// <summary>
/// Результат применения обратного вызова тестирующего модуля
/// </summary>
public readonly record struct TesterCallbackUpdateResult(
    TesterCallbackUpdateStatus Status,
    DbSolution? Solution);

/// <summary>
/// Результат отправки решения во внешний тестирующий модуль.
/// </summary>
public readonly record struct SubmitDispatchResult(bool Success, string? ErrorMessage)
{
    public static SubmitDispatchResult Ok() => new(true, null);

    public static SubmitDispatchResult Failed(string? error) => new(false, error);
}
