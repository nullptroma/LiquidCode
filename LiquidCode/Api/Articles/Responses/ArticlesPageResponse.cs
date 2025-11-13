using System.Collections.Generic;

namespace LiquidCode.Api.Articles.Responses;

/// <summary>
/// Пагинированный ответ для списка статей
/// </summary>
/// <param name="HasNextPage">Есть ли следующая страница</param>
/// <param name="Articles">Список статей на текущей странице</param>
public record ArticlesPageResponse(
    bool HasNextPage,
    IEnumerable<ArticleResponse> Articles
);
