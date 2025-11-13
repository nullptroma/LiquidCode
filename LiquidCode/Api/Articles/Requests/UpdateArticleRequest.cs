namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Запрос на обновление существующей статьи
/// </summary>
/// <param name="Name">Новое название статьи</param>
/// <param name="Tags">Новый список тегов</param>
/// <param name="Content">Новое содержимое статьи</param>
public record UpdateArticleRequest(
    string? Name,
    IEnumerable<string>? Tags,
    string? Content
);
