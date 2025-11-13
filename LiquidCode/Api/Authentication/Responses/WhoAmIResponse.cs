namespace LiquidCode.Api.Authentication.Responses;

/// <summary>
/// Модель ответа, содержащая информацию о текущем пользователе
/// </summary>
/// <param name="Username">Имя пользователя</param>
public record WhoAmIResponse(string Username);
