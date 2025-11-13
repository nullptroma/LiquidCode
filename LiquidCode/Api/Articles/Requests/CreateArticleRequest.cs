namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Запрос на создание новой статьи
/// </summary>
/// <param name="Name">Название статьи</param>
/// <param name="Content">Содержимое статьи</param>
/// <param name="Tags">Список тегов статьи</param>
public record CreateArticleRequest(
    string Name,
    string Content,
    IEnumerable<string>? Tags
);
