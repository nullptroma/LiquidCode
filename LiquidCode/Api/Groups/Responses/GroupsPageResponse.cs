using System.Collections.Generic;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Пагинированный ответ для списка групп
/// </summary>
public record GroupsPageResponse(
    bool HasNextPage,
    IEnumerable<GroupResponse> Groups
);
