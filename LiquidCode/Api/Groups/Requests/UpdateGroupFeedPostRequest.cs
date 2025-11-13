namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на обновление поста ленты
/// </summary>
public record UpdateGroupFeedPostRequest(string? Name, string? Content);
