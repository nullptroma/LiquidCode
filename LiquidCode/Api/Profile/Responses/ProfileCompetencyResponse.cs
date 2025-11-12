namespace LiquidCode.Api.Profile.Responses;

public record ProfileCompetencyResponse(
    string TagName,
    int SolvedMissions,
    int TotalMissions);
