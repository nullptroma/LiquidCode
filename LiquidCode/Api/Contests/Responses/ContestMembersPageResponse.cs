using System.Collections.Generic;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Пагинированный ответ с участниками контеста
/// </summary>
/// <param name="HasNextPage">Есть ли следующая страница</param>
/// <param name="Members">Список участников на текущей странице</param>
public record ContestMembersPageResponse(
    bool HasNextPage,
    IReadOnlyList<ContestMemberResponse> Members
);
