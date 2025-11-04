using System.Collections.Generic;
using System.Linq;
using LiquidCode.Api.Articles.Responses;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Подробный ответ о контесте
/// </summary>
public record ContestResponse(
    int Id,
    string Name,
    string? Description,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool AllowEarlyFinish,
    int? GroupId,
    string? GroupName,
    IReadOnlyList<MissionResponse> Missions,
    IReadOnlyList<ArticleResponse> Articles,
    IReadOnlyList<ContestMemberResponse> Members
)
{
    public static ContestResponse FromEntity(DbContest entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.ScheduleType,
    entity.Visibility,
    entity.StartsAt,
    entity.EndsAt,
        entity.AttemptDurationMinutes,
    entity.MaxAttempts,
    entity.AllowEarlyFinish,
        entity.GroupId,
        entity.Group?.Name,
        entity.Missions
            .OrderBy(m => m.SortOrder)
            .Select(m => MissionResponse.FromEntity(m.Mission, includeStatements: false))
            .ToList(),
        entity.Articles
            .OrderBy(a => a.SortOrder)
            .Select(a => ArticleResponse.FromEntity(a.Article))
            .ToList(),
        entity.Memberships
            .Select(m => new ContestMemberResponse(m.UserId, m.User.Username, m.Role))
            .ToList()
    );
}

/// <summary>
/// Описание участника или организатора контеста
/// </summary>
public record ContestMemberResponse(int UserId, string Username, ContestMembershipRole Role);
