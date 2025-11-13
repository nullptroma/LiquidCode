namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на обновление поста ленты
/// </summary>
/// <param name="Name">Новое название поста</param>
/// <param name="Content">Новое содержимое поста</param>
public record UpdateGroupFeedPostRequest(string? Name, string? Content);
