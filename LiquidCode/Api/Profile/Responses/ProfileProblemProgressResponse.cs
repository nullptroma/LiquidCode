namespace LiquidCode.Api.Profile.Responses;

public record ProfileProblemProgressResponse(
    ProfileProgressCounterResponse Total,
    IReadOnlyList<ProfileDifficultyProgressResponse> Difficulties);
