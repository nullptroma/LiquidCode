namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Запрос на создание новой статьи
/// </summary>
public record CreateArticleRequest(
    string Name,
    string Content,
    IEnumerable<string>? Tags
);
