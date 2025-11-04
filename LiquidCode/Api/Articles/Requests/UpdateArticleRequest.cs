namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Запрос на обновление существующей статьи
/// </summary>
public record UpdateArticleRequest(
    string? Name,
    IEnumerable<string>? Tags,
    string? Content
);
