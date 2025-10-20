namespace LiquidCode.Api.Authentication.Responses;

/// <summary>
/// Response model containing JWT and refresh tokens
/// </summary>
public record AuthTokensResponse(string Jwt, string RefreshToken);
