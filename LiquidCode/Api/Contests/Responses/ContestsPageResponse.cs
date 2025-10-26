using System.Collections.Generic;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Пагинированный ответ для списка контестов
/// </summary>
public record ContestsPageResponse(
    bool HasNextPage,
    IEnumerable<ContestResponse> Contests
);
