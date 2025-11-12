namespace LiquidCode.Api.Profile.Responses;

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
