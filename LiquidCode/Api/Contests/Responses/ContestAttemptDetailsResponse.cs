using System;
using System.Collections.Generic;
using LiquidCode.Api.Submits.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Детальная информация по попытке пользователя в контесте
/// </summary>
/// <param name="AttemptId">Идентификатор попытки</param>
/// <param name="AttemptIndex">Номер попытки (порядковый номер для пользователя)</param>
/// <param name="Status">Статус попытки</param>
/// <param name="FinishedBy">Причина завершения попытки (таймер, вручную, администратором или системой)</param>
/// <param name="ScheduleType">Тип расписания контеста</param>
/// <param name="StartedAt">Дата и время начала попытки</param>
/// <param name="ExpiresAt">Дата и время истечения попытки (если применимо)</param>
/// <param name="FinishedAt">Дата и время завершения попытки</param>
/// <param name="TotalScore">Общий набранный балл</param>
/// <param name="SolvedCount">Количество решенных задач</param>
/// <param name="MissionResults">Результаты по каждой миссии в рамках этой попытки</param>
/// <param name="Submissions">Все посылки пользователя, выполненные в рамках этой попытки</param>
public record ContestAttemptDetailsResponse(
    int AttemptId,
    int AttemptIndex,
    ContestAttemptStatus Status,
    ContestAttemptFinishReason? FinishedBy,
    ContestScheduleType ScheduleType,
    DateTime StartedAt,
    DateTime? ExpiresAt,
    DateTime? FinishedAt,
    decimal TotalScore,
    int SolvedCount,
    IReadOnlyList<ContestAttemptMissionResultResponse> MissionResults,
    IReadOnlyList<SubmissionResponse> Submissions
);
