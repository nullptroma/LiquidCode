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
    GroupJoinLinkResponse? ActiveJoinLink,
    IReadOnlyList<GroupInvitationResponse> PendingInvitations
)
{
    public static GroupResponse FromEntity(
        DbGroup entity,
        bool includePrivateDetails = false,
        DbGroupJoinToken? activeJoinToken = null,
        IEnumerable<DbGroupInvitation>? invitations = null)
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
            .OrderByDescending(c => c.ScheduleType == ContestScheduleType.FixedWindow ? c.StartsAt : c.AvailableFrom)
            .ThenByDescending(c => c.Id)
            .Select(c => new GroupContestSummary(
                c.Id,
                c.Name,
                c.ScheduleType,
                c.Visibility,
                c.StartsAt,
                c.EndsAt,
                c.AvailableFrom,
                c.AvailableUntil,
                c.AttemptDurationMinutes,
                c.MaxAttempts,
                c.AllowEarlyFinish))
            .ToList();

        GroupJoinLinkResponse? joinLink = null;
        if (includePrivateDetails && activeJoinToken != null && activeJoinToken.RevokedAt == null && activeJoinToken.ExpiresAt > now)
        {
            joinLink = new GroupJoinLinkResponse(activeJoinToken.Token, activeJoinToken.ExpiresAt);
        }

        var pendingInvitations = includePrivateDetails && invitations != null
            ? invitations
                .Where(i => i.Status == GroupInvitationStatus.Pending && i.ExpiresAt > now && i.RevokedAt == null)
                .Select(i => new GroupInvitationResponse(
                    i.Id,
                    i.InviteeId,
                    i.Invitee.Username,
                    i.Status,
                    i.ExpiresAt,
                    i.CreatedAt))
                .OrderByDescending(i => i.CreatedAt)
                .ToList()
            : new List<GroupInvitationResponse>();

        return new GroupResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            members,
            contests,
            joinLink,
            pendingInvitations);
    }
}

/// <summary>
/// Информация об участнике группы
/// </summary>
public record GroupMemberResponse(int UserId, string Username, GroupMembershipRole Role, DateTime JoinedAt, bool IsAutoJoined);

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
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool AllowEarlyFinish
);

/// <summary>
/// Активный токен присоединения к группе
/// </summary>
public record GroupJoinLinkResponse(string Token, DateTime ExpiresAt);

/// <summary>
/// Приглашение в группу
/// </summary>
public record GroupInvitationResponse(
    int InvitationId,
    int InviteeId,
    string InviteeUsername,
    GroupInvitationStatus Status,
    DateTime ExpiresAt,
    DateTime CreatedAt
);
