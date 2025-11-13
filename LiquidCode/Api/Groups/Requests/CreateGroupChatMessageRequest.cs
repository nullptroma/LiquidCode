namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на отправку сообщения в чате
/// </summary>
/// <param name="Content">Текст сообщения</param>
public record CreateGroupChatMessageRequest(string Content);
