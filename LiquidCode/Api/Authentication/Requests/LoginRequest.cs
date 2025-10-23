namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Модель запроса для входа пользователя
/// </summary>
public record LoginRequest(string Username, string Password);
