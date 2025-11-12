namespace LiquidCode.Api.Profile.Responses;

public record ProfileDifficultyProgressResponse(
    string Key,
    string Label,
    int Completed,
    int Total);
