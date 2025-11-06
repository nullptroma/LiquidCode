using System.Collections.Generic;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Пагинированный ответ с участниками контеста
/// </summary>
public record ContestMembersPageResponse(
    bool HasNextPage,
    IReadOnlyList<ContestMemberResponse> Members
);
