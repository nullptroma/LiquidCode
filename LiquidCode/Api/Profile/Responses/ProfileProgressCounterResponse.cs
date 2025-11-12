namespace LiquidCode.Api.Profile.Responses;

public record ProfileProgressCounterResponse(
    string Key,
    string Label,
    int Completed,
    int Total);
