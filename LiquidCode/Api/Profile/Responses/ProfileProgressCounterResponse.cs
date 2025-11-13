namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Счетчик прогресса с ключом и меткой
/// </summary>
/// <param name="Key">Уникальный ключ счетчика</param>
/// <param name="Label">Человекочитаемая метка счетчика</param>
/// <param name="Completed">Количество выполненных элементов</param>
/// <param name="Total">Общее количество элементов</param>
public record ProfileProgressCounterResponse(
    string Key,
    string Label,
    int Completed,
    int Total);
