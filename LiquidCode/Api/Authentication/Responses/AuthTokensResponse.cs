namespace LiquidCode.Api.Authentication.Responses;

/// <summary>
/// Модель ответа, содержащая JWT и токены обновления
/// </summary>
/// <param name="Jwt">JWT токен для авторизации</param>
/// <param name="RefreshToken">Токен для обновления JWT</param>
public record AuthTokensResponse(string Jwt, string RefreshToken);
