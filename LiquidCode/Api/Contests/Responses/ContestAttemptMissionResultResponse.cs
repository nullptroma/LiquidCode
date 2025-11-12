using System;

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
