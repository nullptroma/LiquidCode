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
    IReadOnlyList<GroupContestSummary> Contests,
    GroupJoinLinkResponse? ActiveJoinLink
)
{
    public static GroupResponse FromEntity(
        DbGroup entity,
        bool includePrivateDetails = false,
        DbGroupJoinToken? activeJoinToken = null)
    {
        var now = DateTime.UtcNow;

        var members = entity.Memberships
            .Select(m => new GroupMemberResponse(
                m.UserId,
                m.User.Username,
                m.Role,
                m.JoinedAt,
                m.IsAutoJoined))
            .OrderByDescending(m => m.Role.HasFlag(GroupMembershipRole.Creator))
            .ThenByDescending(m => m.Role.HasFlag(GroupMembershipRole.Administrator))
            .ThenBy(m => m.JoinedAt)
            .ToList();

        var contests = entity.Contests
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.ScheduleType == ContestScheduleType.FixedWindow ? c.StartsAt : c.StartsAt ?? c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Select(c => new GroupContestSummary(
                c.Id,
                c.Name,
                c.ScheduleType,
                c.Visibility,
                c.StartsAt,
                c.EndsAt,
                c.AttemptDurationMinutes,
                c.MaxAttempts,
                c.AllowEarlyFinish))
            .ToList();

        GroupJoinLinkResponse? joinLink = null;
        if (includePrivateDetails && activeJoinToken != null && activeJoinToken.RevokedAt == null && activeJoinToken.ExpiresAt > now)
        {
            joinLink = new GroupJoinLinkResponse(activeJoinToken.Token, activeJoinToken.ExpiresAt);
        }

        return new GroupResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            members,
            contests,
            joinLink);
    }
}
