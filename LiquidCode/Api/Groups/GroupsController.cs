using System.ComponentModel.DataAnnotations;
using LiquidCode.Api.Groups.Requests;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Groups;

/// <summary>
/// Контроллер управления группами
/// </summary>
[Route("groups")]
[ApiController]
public class GroupsController(IGroupService groupService) : ControllerBase
{
    /// <summary>
    /// Создает новую группу
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await groupService.CreateAsync(request, userId, cancellationToken);
        if (result == null)
            return BadRequest("Unable to create group.");

        return Ok(result);
    }

    /// <summary>
    /// Обновляет данные группы
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await groupService.UpdateAsync(id, request, userId, cancellationToken);
        if (result == null)
            return NotFound("Group not found or access denied.");

        return Ok(result);
    }

    /// <summary>
    /// Удаляет группу (мягкое удаление)
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var success = await groupService.DeleteAsync(id, userId, cancellationToken);
        if (!success)
            return NotFound("Group not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Возвращает информацию о конкретной группе
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var requesterId = User.TryGetUserId(out var userId) ? userId : (int?)null;
        var result = await groupService.GetAsync(id, requesterId, cancellationToken);
        if (result == null)
            return NotFound("Group not found.");

        return Ok(result);
    }

    /// <summary>
    /// Возвращает список групп текущего пользователя
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroups(
        [FromQuery] [Range(1, 100)] int pageSize = 10,
        [FromQuery] [Range(0, int.MaxValue)] int page = 0,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await groupService.GetForUserAsync(userId, pageSize, page, cancellationToken);
        if (result == null)
            return BadRequest("Invalid pagination parameters.");

        return Ok(result);
    }

    /// <summary>
    /// Добавляет или обновляет участника группы
    /// </summary>
    [Authorize]
    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> UpdateMemberRole([FromRoute] int id, [FromBody] GroupMembershipRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await groupService.UpdateMemberRoleAsync(id, userId, request.UserId, request.Role, cancellationToken);
        if (!success)
            return NotFound("Group not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Удаляет участника группы
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}/members/{memberId:int}")]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int memberId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var success = await groupService.RemoveMemberAsync(id, userId, memberId, cancellationToken);
        if (!success)
            return NotFound("Group not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Возвращает актуальную ссылку для присоединения к группе
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}/join-link")]
    public async Task<IActionResult> GetJoinLink([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await groupService.GetJoinLinkAsync(id, userId, cancellationToken);
        if (result == null)
            return NotFound("Group not found or access denied.");

        return Ok(result);
    }

    /// <summary>
    /// Присоединение к группе по приглашению-ссылке
    /// </summary>
    [Authorize]
    [HttpPost("join/{token}")]
    public async Task<IActionResult> JoinByToken([FromRoute] string token, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var response = await groupService.JoinByTokenAsync(token, userId, cancellationToken);
        if (response == null)
            return BadRequest("Join token is invalid or expired.");

        return Ok(response);
    }
}
