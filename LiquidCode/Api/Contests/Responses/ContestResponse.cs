using System.Collections.Generic;
using System.Linq;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Подробный ответ о контесте
/// </summary>
public record ContestResponse(
    int Id,
    string Name,
    string? Description,
    DateTime StartsAt,
    DateTime EndsAt,
    int? GroupId,
    string? GroupName,
    IReadOnlyList<ContestMissionResponse> Missions,
    IReadOnlyList<ContestArticleResponse> Articles,
    IReadOnlyList<ContestMemberResponse> Members
)
{
    public static ContestResponse FromEntity(DbContest entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.StartsAt,
        entity.EndsAt,
        entity.GroupId,
        entity.Group?.Name,
        entity.Missions
            .OrderBy(m => m.SortOrder)
            .Select(m => new ContestMissionResponse(m.MissionId, m.Mission.Name, m.SortOrder))
            .ToList(),
        entity.Articles
            .OrderBy(a => a.SortOrder)
            .Select(a => new ContestArticleResponse(a.ArticleId, a.Article.Name, a.SortOrder))
            .ToList(),
        entity.Memberships
            .Select(m => new ContestMemberResponse(m.UserId, m.User.Username, m.Role))
            .ToList()
    );
}

/// <summary>
/// Короткое описание миссии внутри контеста
/// </summary>
public record ContestMissionResponse(int MissionId, string Name, int SortOrder);

/// <summary>
/// Короткое описание статьи внутри контеста
/// </summary>
public record ContestArticleResponse(int ArticleId, string Name, int SortOrder);

/// <summary>
/// Описание участника или организатора контеста
/// </summary>
public record ContestMemberResponse(int UserId, string Username, ContestMembershipRole Role);
