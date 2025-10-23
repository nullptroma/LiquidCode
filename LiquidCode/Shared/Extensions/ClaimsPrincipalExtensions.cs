using System.Security.Claims;

namespace LiquidCode.Shared.Extensions;

/// <summary>
/// Методы расширения для работы с ClaimsPrincipal (пользовательские claims)
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Пытается извлечь ID пользователя из claims
    /// </summary>
    /// <param name="user">ClaimsPrincipal для извлечения</param>
    /// <param name="userId">Выходной параметр для извлеченного ID пользователя</param>
    /// <returns>True, если ID пользователя найден и успешно распарсен, иначе false</returns>
    public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
    {
        userId = 0;
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out userId);
    }

    /// <summary>
    /// Получает ID пользователя из claims, или возвращает null, если не найден
    /// </summary>
    public static int? GetUserIdOrNull(this ClaimsPrincipal user)
    {
        return user.TryGetUserId(out var userId) ? userId : null;
    }

    /// <summary>
    /// Получает имя пользователя из claims
    /// </summary>
    public static string? GetUsername(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Получает email из claims
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Email)?.Value;
}
