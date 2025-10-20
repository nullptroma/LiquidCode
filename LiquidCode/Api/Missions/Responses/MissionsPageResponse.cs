namespace LiquidCode.Api.Missions.Responses;

/// <summary>
/// Paginated response for missions list
/// </summary>
public record MissionsPageResponse(
    bool HasNextPage, 
    IEnumerable<MissionResponse> Missions
);
