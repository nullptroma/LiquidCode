using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на создание нового контеста
/// </summary>
public record CreateContestRequest(
    [Required] [StringLength(128, MinimumLength = 3)] string Name,
    string? Description,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool? AllowEarlyFinish,
    int? GroupId,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds,
    IEnumerable<int>? ParticipantIds,
    IEnumerable<int>? OrganizerIds
);
