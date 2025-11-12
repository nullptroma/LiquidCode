namespace LiquidCode.Api.Profile.Responses;

public record ProfileHeaderResponse(
    int UserId,
    string Username,
    string Email,
    DateTime RegisteredAt,
    double? TopPercent,
    string? AvatarUrl);
