namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Пагинированный ответ для ленты группы
/// </summary>
/// <param name="HasNext">Есть ли следующая страница</param>
/// <param name="Items">Список постов на текущей странице</param>
public record GroupFeedPageResponse(bool HasNext, IReadOnlyList<GroupFeedPostResponse> Items);
