namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Метрика активности с подсчетом за все время и за последние 7 дней
/// </summary>
/// <param name="Label">Человекочитаемая метка метрики</param>
/// <param name="TotalCount">Общее количество за все время</param>
/// <param name="Last7DaysCount">Количество за последние 7 дней</param>
public record ProfileActivityMetricResponse(
    string Label,
    int TotalCount,
    int Last7DaysCount);
