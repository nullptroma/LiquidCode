namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на создание поста в ленте группы
/// </summary>
/// <param name="Name">Название поста</param>
/// <param name="Content">Содержимое поста</param>
public record CreateGroupFeedPostRequest(string Name, string Content);
