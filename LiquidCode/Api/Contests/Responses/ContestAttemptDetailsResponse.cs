using System;
using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Детальная информация по попытке пользователя в контесте
/// </summary>
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
    IReadOnlyList<ContestAttemptMissionResultResponse> MissionResults
);
