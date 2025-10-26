using LiquidCode.Api.Contests.Requests;
using LiquidCode.Domain.Services.Contests;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Contests;

/// <summary>
/// Контроллер управления контестами
/// </summary>
[Route("contests")]
[ApiController]
public class ContestsController(IContestService contestService) : ControllerBase
{
    /// <summary>
    /// Создает новый контест
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContestRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await contestService.CreateAsync(request, userId, cancellationToken);
        if (result == null)
            return BadRequest("Unable to create contest.");

        return Ok(result);
    }

    /// <summary>
    /// Обновляет параметры контеста
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateContestRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await contestService.UpdateAsync(id, request, userId, cancellationToken);
        if (result == null)
            return NotFound("Contest not found or access denied.");

        return Ok(result);
    }

    /// <summary>
    /// Удаляет контест (мягкое удаление)
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var success = await contestService.DeleteAsync(id, userId, cancellationToken);
        if (!success)
            return NotFound("Contest not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Добавляет или обновляет участника контеста
    /// </summary>
    [Authorize]
    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> UpsertMember([FromRoute] int id, [FromBody] ContestMembershipRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await contestService.UpsertMemberAsync(id, userId, request.UserId, request.Role, cancellationToken);
        if (!success)
            return NotFound("Contest not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Удаляет участника контеста
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}/members/{memberId:int}")]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int memberId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var success = await contestService.RemoveMemberAsync(id, userId, memberId, cancellationToken);
        if (!success)
            return NotFound("Contest not found or access denied.");

        return NoContent();
    }

    /// <summary>
    /// Запускает персональную попытку контеста для текущего пользователя
    /// </summary>
    [Authorize]
    [HttpPost("{id:int}/attempts")]
    public async Task<IActionResult> StartAttempt([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await contestService.StartAttemptAsync(id, userId, cancellationToken);
        if (result == null)
            return BadRequest("Unable to start attempt or contest not available.");

        return Ok(result);
    }

    /// <summary>
    /// Возвращает список контестов (глобальный или по группе)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageSize = 10,
        [FromQuery] int page = 0,
        [FromQuery] int? groupId = null,
        CancellationToken cancellationToken = default)
    {
        var result = groupId.HasValue
            ? await contestService.GetForGroupAsync(groupId.Value, pageSize, page, cancellationToken)
            : await contestService.GetUpcomingAsync(pageSize, page, cancellationToken);

        if (result == null)
            return BadRequest("Invalid pagination parameters.");

        return Ok(result);
    }

    /// <summary>
    /// Возвращает подробную информацию о контесте
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await contestService.GetAsync(id, cancellationToken);
        if (result == null)
            return NotFound("Contest not found.");

        return Ok(result);
    }
}
