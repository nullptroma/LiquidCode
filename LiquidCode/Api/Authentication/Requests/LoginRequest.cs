namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Модель запроса для входа пользователя
/// </summary>
/// <param name="Username">Имя пользователя</param>
/// <param name="Password">Пароль</param>
public record LoginRequest(
    string Username,
    string Password
    );
