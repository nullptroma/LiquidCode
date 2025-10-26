using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на создание нового контеста
/// </summary>
public record CreateContestRequest(
    [Required] [StringLength(128, MinimumLength = 3)] string Name,
    string? Description,
    DateTime StartsAt,
    DateTime EndsAt,
    int? GroupId,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds,
    IEnumerable<int>? ParticipantIds,
    IEnumerable<int>? OrganizerIds
);
