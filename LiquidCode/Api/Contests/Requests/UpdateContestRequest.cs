namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на обновление контеста
/// </summary>
public record UpdateContestRequest(
    string? Name,
    string? Description,
    DateTime? StartsAt,
    DateTime? EndsAt,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds
);
