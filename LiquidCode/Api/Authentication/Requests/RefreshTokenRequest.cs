namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Модель запроса для обновления JWT токена
/// </summary>
public record RefreshTokenRequest(string RefreshToken);
