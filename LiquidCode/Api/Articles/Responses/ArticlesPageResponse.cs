using System.Collections.Generic;

namespace LiquidCode.Api.Articles.Responses;

/// <summary>
/// Пагинированный ответ для списка статей
/// </summary>
public record ArticlesPageResponse(
    bool HasNextPage,
    IEnumerable<ArticleResponse> Articles
);
