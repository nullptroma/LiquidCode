namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Модель запроса для регистрации пользователя
/// </summary>
public record RegisterRequest(string Username, string Email, string Password);
