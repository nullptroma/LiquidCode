namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Пагинированный ответ для списка миссий
/// </summary>
public record MissionsPageResponse(
    bool HasNextPage, 
    IEnumerable<MissionResponse> Missions
);
