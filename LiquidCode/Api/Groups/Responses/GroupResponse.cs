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
            .OrderByDescending(c => c.StartsAt)
            .Select(c => new GroupContestSummary(c.Id, c.Name, c.StartsAt, c.EndsAt))
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
public record GroupContestSummary(int ContestId, string Name, DateTime StartsAt, DateTime EndsAt);
