namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Активность пользователя
/// </summary>
/// <param name="Solutions">Активность по решению задач</param>
/// <param name="Creation">Активность по созданию контента</param>
public record ProfileActivityResponse(
    ProfileSolutionActivityResponse Solutions,
    ProfileCreationActivityResponse Creation);
