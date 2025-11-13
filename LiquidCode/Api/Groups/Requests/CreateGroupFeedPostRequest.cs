namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на создание поста в ленте группы
/// </summary>
public record CreateGroupFeedPostRequest(string Name, string Content);
