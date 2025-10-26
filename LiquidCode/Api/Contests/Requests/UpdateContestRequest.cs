using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на обновление контеста
/// </summary>
public record UpdateContestRequest(
    string? Name,
    string? Description,
    ContestScheduleType? ScheduleType,
    DateTime? StartsAt,
    DateTime? EndsAt,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    int? AttemptDurationMinutes,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds
);
