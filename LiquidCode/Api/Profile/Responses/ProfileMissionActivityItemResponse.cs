namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Элемент активности пользователя по миссии
/// </summary>
/// <param name="MissionId">Идентификатор миссии</param>
/// <param name="MissionName">Название миссии</param>
/// <param name="DifficultyLabel">Метка уровня сложности</param>
/// <param name="DifficultyValue">Числовое значение сложности</param>
/// <param name="IsSuccessful">Успешность решения</param>
/// <param name="Status">Статус решения</param>
/// <param name="CreatedAt">Дата и время создания попытки</param>
/// <param name="TimeLimitMilliseconds">Ограничение по времени в миллисекундах</param>
/// <param name="MemoryLimitBytes">Ограничение по памяти в байтах</param>
public record ProfileMissionActivityItemResponse(
    int MissionId,
    string MissionName,
    string DifficultyLabel,
    int DifficultyValue,
    bool? IsSuccessful,
    string Status,
    DateTime CreatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);
