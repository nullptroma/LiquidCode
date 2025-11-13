using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Ответ с информацией о попытке пользователя в контесте
/// </summary>
/// <param name="AttemptId">Идентификатор попытки</param>
/// <param name="ContestId">Идентификатор контеста</param>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="AttemptIndex">Номер попытки</param>
/// <param name="Status">Статус попытки</param>
/// <param name="ScheduleType">Тип расписания контеста</param>
/// <param name="StartedAt">Дата и время начала попытки</param>
/// <param name="ExpiresAt">Дата и время истечения попытки</param>
/// <param name="FinishedAt">Дата и время завершения попытки</param>
/// <param name="TotalScore">Общий набранный балл</param>
/// <param name="SolvedCount">Количество решенных задач</param>
public record ContestAttemptResponse(
    int AttemptId,
    int ContestId,
    int UserId,
    int AttemptIndex,
    ContestAttemptStatus Status,
    ContestScheduleType ScheduleType,
    DateTime StartedAt,
    DateTime? ExpiresAt,
    DateTime? FinishedAt,
    decimal TotalScore,
    int SolvedCount
);
