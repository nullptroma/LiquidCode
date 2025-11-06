using System;
using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Информация о результате миссии в рамках попытки контеста
/// </summary>
public record ContestAttemptMissionResultResponse(
    int MissionId,
    string MissionName,
    DateTime? SolvedAt,
    bool IsSolved,
    decimal HighestScore,
    int SubmissionCount,
    double Penalty,
    DateTime? FirstAcceptedAt,
    DateTime? LastSubmissionAt,
    int? BestSubmissionId
);

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
