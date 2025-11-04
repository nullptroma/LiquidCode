using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Ответ с информацией о попытке пользователя в контесте
/// </summary>
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
