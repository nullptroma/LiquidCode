using System;
using System.Collections.Generic;
using System.Linq;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Ответ о группе
/// </summary>
public record GroupResponse(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<GroupMemberResponse> Members,
    IReadOnlyList<GroupContestSummary> Contests
)
{
    public static GroupResponse FromEntity(DbGroup entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.Memberships
            .Select(m => new GroupMemberResponse(m.UserId, m.User.Username, m.Role))
            .ToList(),
        entity.Contests
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.ScheduleType == ContestScheduleType.FixedWindow ? c.StartsAt : c.AvailableFrom)
            .ThenByDescending(c => c.Id)
            .Select(c => new GroupContestSummary(
                c.Id,
                c.Name,
                c.ScheduleType,
                c.StartsAt,
                c.EndsAt,
                c.AvailableFrom,
                c.AvailableUntil,
                c.AttemptDurationMinutes))
            .ToList()
    );
}

/// <summary>
/// Информация об участнике группы
/// </summary>
public record GroupMemberResponse(int UserId, string Username, GroupMembershipRole Role);

/// <summary>
/// Краткое описание контеста, созданного в группе
/// </summary>
public record GroupContestSummary(
    int ContestId,
    string Name,
    ContestScheduleType ScheduleType,
    DateTime? StartsAt,
    DateTime? EndsAt,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    int? AttemptDurationMinutes
);
