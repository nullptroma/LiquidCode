using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на обновление контеста
/// </summary>
public record UpdateContestRequest(
    string? Name,
    string? Description,
    ContestScheduleType? ScheduleType,
    ContestVisibility? Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool? AllowEarlyFinish,
    int? GroupId,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds
);
