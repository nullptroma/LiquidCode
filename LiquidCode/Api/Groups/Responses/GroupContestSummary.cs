using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Краткое описание контеста, созданного в группе
/// </summary>
public record GroupContestSummary(
    int ContestId,
    string Name,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool AllowEarlyFinish
);
