namespace LiquidCode.Api.Profile.Responses;

public record ProfileSolutionActivityResponse(
    ProfileActivityMetricResponse Problems,
    ProfileActivityMetricResponse Contests);
