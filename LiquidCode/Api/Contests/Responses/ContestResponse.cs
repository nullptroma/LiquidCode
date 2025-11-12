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
    IReadOnlyList<ArticleResponse> Articles
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
            .Select(m => m.Mission)
            .Where(m => m != null)
            .Select(m => MissionResponse.FromEntity(m!, includeStatements: false))
            .ToList(),
        entity.Articles
            .OrderBy(a => a.SortOrder)
            .Select(a => a.Article)
            .Where(a => a != null)
            .Select(a => ArticleResponse.FromEntity(a!))
            .ToList()
    );
}
