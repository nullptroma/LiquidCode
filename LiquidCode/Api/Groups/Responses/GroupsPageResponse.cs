using System.Collections.Generic;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Пагинированный ответ для списка групп
/// </summary>
/// <param name="HasNextPage">Есть ли следующая страница</param>
/// <param name="Groups">Список групп на текущей странице</param>
public record GroupsPageResponse(
    bool HasNextPage,
    IEnumerable<GroupResponse> Groups
);
