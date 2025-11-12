namespace LiquidCode.Api.Profile.Responses;

public record ProfileAuthoredMissionResponse(
    int MissionId,
    string MissionName,
    string DifficultyLabel,
    int DifficultyValue,
    DateTime CreatedAt,
    int? TimeLimitMilliseconds,
    int? MemoryLimitBytes);
