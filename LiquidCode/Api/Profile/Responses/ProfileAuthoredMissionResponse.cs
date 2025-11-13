namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Миссия, созданная пользователем
/// </summary>
/// <param name="MissionId">Идентификатор миссии</param>
/// <param name="MissionName">Название миссии</param>
/// <param name="DifficultyLabel">Метка уровня сложности</param>
/// <param name="DifficultyValue">Числовое значение сложности</param>
/// <param name="CreatedAt">Дата и время создания миссии</param>
/// <param name="TimeLimitMilliseconds">Ограничение по времени в миллисекундах</param>
/// <param name="MemoryLimitBytes">Ограничение по памяти в байтах</param>
public record ProfileAuthoredMissionResponse(
    int MissionId,
    string MissionName,
    string DifficultyLabel,
    int DifficultyValue,
    DateTime CreatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);
