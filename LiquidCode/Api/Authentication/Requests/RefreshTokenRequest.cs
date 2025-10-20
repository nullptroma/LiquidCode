namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Request model for refreshing JWT token
/// </summary>
public record RefreshTokenRequest(string RefreshToken);
