namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Модель запроса для регистрации пользователя
/// </summary>
/// <param name="Username">Имя пользователя</param>
/// <param name="Email">Email пользователя</param>
/// <param name="Password">Пароль</param>
public record RegisterRequest(string Username, string Email, string Password);
