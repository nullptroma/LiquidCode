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
    [HttpGet]
    public async Task<IActionResult> GetMessages(
        [FromRoute] int groupId,
        [FromQuery] int limit = 50,
        [FromQuery] long? afterMessageId = null,
        [FromQuery] DateTime? afterCreatedAt = null,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (limit <= 0)
            return BadRequest("Limit must be positive.");

        var result = await chatService.GetMessagesAsync(groupId, userId, limit, afterMessageId, afterCreatedAt, cancellationToken);
        if (result == null)
            return NotFound("Group not found or access denied.");

        return Ok(result);
    }

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
