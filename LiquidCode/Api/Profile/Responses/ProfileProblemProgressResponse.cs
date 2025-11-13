namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Прогресс решения задач с разбивкой по сложности
/// </summary>
/// <param name="Total">Общий прогресс по всем задачам</param>
/// <param name="Difficulties">Прогресс по уровням сложности</param>
public record ProfileProblemProgressResponse(
    ProfileProgressCounterResponse Total,
    IReadOnlyList<ProfileDifficultyProgressResponse> Difficulties);
