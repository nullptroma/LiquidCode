namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Прогресс по определенному уровню сложности
/// </summary>
/// <param name="Key">Ключ уровня сложности</param>
/// <param name="Label">Человекочитаемая метка уровня сложности</param>
/// <param name="Completed">Количество решенных задач</param>
/// <param name="Total">Общее количество задач данной сложности</param>
public record ProfileDifficultyProgressResponse(
    string Key,
    string Label,
    int Completed,
    int Total);
