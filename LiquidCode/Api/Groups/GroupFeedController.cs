using LiquidCode.Api.Groups.Requests;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Groups;

/// <summary>
/// Контроллер ленты группы
/// </summary>
[Route("groups/{groupId:int}/feed")]
[ApiController]
[Authorize]
public class GroupFeedController(IGroupFeedService feedService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPage(
        [FromRoute] int groupId,
        [FromQuery] int pageSize = 20,
        [FromQuery] int page = 0,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (pageSize <= 0 || page < 0)
            return BadRequest("Invalid pagination parameters.");

        var result = await feedService.GetPageAsync(groupId, userId, pageSize, page, cancellationToken);
        if (result == null)
            return NotFound("Group not found or access denied.");

        return Ok(result);
    }

    [HttpGet("{postId:int}")]
    public async Task<IActionResult> Get(
        [FromRoute] int groupId,
        [FromRoute] int postId,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await feedService.GetAsync(groupId, postId, userId, cancellationToken);
        if (result == null)
            return NotFound("Post not found or access denied.");

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute] int groupId,
        [FromBody] CreateGroupFeedPostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await feedService.CreateAsync(groupId, userId, request, cancellationToken);
        if (result == null)
            return NotFound("Group not found or access denied.");

        return Ok(result);
    }

    [HttpPut("{postId:int}")]
    public async Task<IActionResult> Update(
        [FromRoute] int groupId,
        [FromRoute] int postId,
        [FromBody] UpdateGroupFeedPostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await feedService.UpdateAsync(groupId, postId, userId, request, cancellationToken);
        if (result == null)
            return NotFound("Post not found or access denied.");

        return Ok(result);
    }

    [HttpDelete("{postId:int}")]
    public async Task<IActionResult> Delete(
        [FromRoute] int groupId,
        [FromRoute] int postId,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var success = await feedService.DeleteAsync(groupId, postId, userId, cancellationToken);
        if (!success)
            return NotFound("Post not found or access denied.");

        return NoContent();
    }
}
