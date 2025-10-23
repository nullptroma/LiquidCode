namespace LiquidCode.Api.Authentication.Responses;

/// <summary>
/// Модель ответа, содержащая JWT и токены обновления
/// </summary>
public record AuthTokensResponse(string Jwt, string RefreshToken);
