namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Пагинированный ответ для ленты группы
/// </summary>
public record GroupFeedPageResponse(bool HasNext, IReadOnlyList<GroupFeedPostResponse> Items);
