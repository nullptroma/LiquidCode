namespace LiquidCode.Api.Profile.Responses;

public record ProfileDetailsResponse(
    ProfileHeaderResponse Header,
    ProfileProblemProgressResponse Problems,
    IReadOnlyList<ProfileCompetencyResponse> Competencies,
    IReadOnlyList<ProfileMissionActivityItemResponse> RecentSubmissions,
    IReadOnlyList<ProfileAuthoredMissionResponse> AuthoredMissions,
    ProfileActivityResponse Activity);
