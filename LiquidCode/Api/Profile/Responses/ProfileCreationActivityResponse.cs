namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Активность пользователя по созданию контента
/// </summary>
/// <param name="Missions">Метрики создания миссий</param>
/// <param name="Articles">Метрики создания статей</param>
/// <param name="Contests">Метрики создания контестов</param>
public record ProfileCreationActivityResponse(
    ProfileActivityMetricResponse Missions,
    ProfileActivityMetricResponse Articles,
    ProfileActivityMetricResponse Contests);
