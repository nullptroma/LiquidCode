namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Пагинированный ответ для списка миссий
/// </summary>
/// <param name="HasNextPage">Есть ли следующая страница</param>
/// <param name="Missions">Список миссий на текущей странице</param>
public record MissionsPageResponse(
    bool HasNextPage, 
    IEnumerable<MissionResponse> Missions
);
