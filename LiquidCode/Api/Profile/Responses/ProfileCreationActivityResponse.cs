namespace LiquidCode.Api.Profile.Responses;

public record ProfileCreationActivityResponse(
    ProfileActivityMetricResponse Missions,
    ProfileActivityMetricResponse Articles,
    ProfileActivityMetricResponse Contests);
