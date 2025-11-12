namespace LiquidCode.Api.Profile.Responses;

public record ProfileActivityResponse(
    ProfileSolutionActivityResponse Solutions,
    ProfileCreationActivityResponse Creation);
