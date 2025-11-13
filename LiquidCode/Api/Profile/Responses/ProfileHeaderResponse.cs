namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Основная информация о профиле пользователя
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Username">Имя пользователя</param>
/// <param name="Email">Email пользователя</param>
/// <param name="RegisteredAt">Дата и время регистрации</param>
/// <param name="TopPercent">Процентиль рейтинга пользователя (какой процент пользователей имеет меньший рейтинг)</param>
/// <param name="AvatarUrl">URL аватара пользователя</param>
public record ProfileHeaderResponse(
    int UserId,
    string Username,
    string Email,
    DateTime RegisteredAt,
    double? TopPercent,
    string? AvatarUrl);
