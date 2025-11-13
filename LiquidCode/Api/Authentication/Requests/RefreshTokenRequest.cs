namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Модель запроса для обновления JWT токена
/// </summary>
/// <param name="RefreshToken">Токен обновления</param>
public record RefreshTokenRequest(string RefreshToken);
