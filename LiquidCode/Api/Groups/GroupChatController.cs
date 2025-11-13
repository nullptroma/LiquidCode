using LiquidCode.Api.Groups.Requests;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Groups;

/// <summary>
/// Контроллер группового чата
/// </summary>
[Route("groups/{groupId:int}/chat")]
[ApiController]
[Authorize]
public class GroupChatController(IGroupChatService chatService) : ControllerBase
{
    /// <summary>
    /// Получить сообщения из чата группы.
    /// Поддерживает два режима работы:
    /// 1. Получение последних N сообщений (если afterMessageId и afterCreatedAt не указаны)
    /// 2. Long polling для новых сообщений (если указан afterMessageId или afterCreatedAt и timeoutSeconds > 0)
    /// </summary>
    /// <param name="groupId">ID группы</param>
    /// <param name="limit">Максимальное количество сообщений (по умолчанию 50)</param>
    /// <param name="afterMessageId">ID сообщения, после которого нужно получить новые. Если null, вернёт последние сообщения.</param>
    /// <param name="afterCreatedAt">Дата создания, после которой нужно получить сообщения. Если null, вернёт последние сообщения.</param>
    /// <param name="timeoutSeconds">Таймаут ожидания новых сообщений в секундах (0-60). Работает только с afterMessageId/afterCreatedAt. По умолчанию 30.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список сообщений в хронологическом порядке (от старых к новым)</returns>
    /// <remarks>
    /// Примеры использования:
    /// - GET /groups/1/chat?limit=20 - получить последние 20 сообщений
    /// - GET /groups/1/chat?afterMessageId=100&amp;timeoutSeconds=30 - ждать новые сообщения после ID 100 до 30 секунд (long polling)
    /// - GET /groups/1/chat?afterMessageId=100&amp;timeoutSeconds=0 - сразу получить новые сообщения после ID 100 без ожидания
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetMessages(
        [FromRoute] int groupId,
        [FromQuery] int limit = 50,
        [FromQuery] long? afterMessageId = null,
        [FromQuery] DateTime? afterCreatedAt = null,
        [FromQuery] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (limit <= 0)
            return BadRequest("Limit must be positive.");

        if (timeoutSeconds < 0 || timeoutSeconds > 60)
            return BadRequest("Timeout must be between 0 and 60 seconds.");

        var result = await chatService.GetMessagesAsync(groupId, userId, limit, afterMessageId, afterCreatedAt, timeoutSeconds, cancellationToken);
        if (result == null)
            return NotFound("Group not found or access denied.");

        return Ok(result);
    }

    /// <summary>
    /// Отправить сообщение в чат группы
    /// </summary>
    /// <param name="groupId">ID группы</param>
    /// <param name="request">Данные сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Отправленное сообщение</returns>
    [HttpPost]
    public async Task<IActionResult> SendMessage(
        [FromRoute] int groupId,
        [FromBody] CreateGroupChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await chatService.SendMessageAsync(groupId, userId, request, cancellationToken);
        if (response == null)
            return NotFound("Group not found or access denied.");

        return Ok(response);
    }
}
