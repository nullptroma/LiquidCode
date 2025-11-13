namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Активность пользователя по решению задач
/// </summary>
/// <param name="Problems">Метрики решения задач</param>
/// <param name="Contests">Метрики участия в контестах</param>
public record ProfileSolutionActivityResponse(
    ProfileActivityMetricResponse Problems,
    ProfileActivityMetricResponse Contests);
