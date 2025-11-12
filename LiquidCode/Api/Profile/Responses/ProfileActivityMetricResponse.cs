namespace LiquidCode.Api.Profile.Responses;

public record ProfileActivityMetricResponse(
    string Label,
    int TotalCount,
    int Last7DaysCount);
