using System.Collections.Generic;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Пагинированный ответ для списка контестов
/// </summary>
/// <param name="HasNextPage">Есть ли следующая страница</param>
/// <param name="Contests">Список контестов на текущей странице</param>
public record ContestsPageResponse(
    bool HasNextPage,
    IEnumerable<ContestResponse> Contests
);
