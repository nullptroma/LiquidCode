using System.ComponentModel.DataAnnotations;
using System.Linq;
using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Submits.Responses;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Domain.Services.Contests;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Contests;

/// <summary>
/// Контроллер управления контестами
/// </summary>
[Route("contests")]
[ApiController]
public class ContestsController(IContestService contestService, ISubmitService submitService) : ControllerBase
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

        try
        {
            var result = await contestService.CreateAsync(request, userId, cancellationToken);
            if (result == null)
                return StatusCode(500, "Contest was created but could not be loaded.");

            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ContestValidationException ex)
        {
            return BadRequest(ex.Message);
        }
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

        try
        {
            var result = await contestService.UpdateAsync(id, request, userId, cancellationToken);
            if (result == null)
                return NotFound("Contest not found or access denied.");

            return Ok(result);
        }
        catch (ContestValidationException ex)
        {
            return BadRequest(ex.Message);
        }
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

        var targetUserId = request?.UserId ?? userId;
        var requestedRole = request?.Role ?? ContestMembershipRole.Participant;

        var success = await contestService.UpsertMemberAsync(id, userId, targetUserId, requestedRole, cancellationToken);
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
        [FromQuery] [Range(1, 100)] int pageSize = 10,
        [FromQuery] [Range(0, int.MaxValue)] int page = 0,
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
    /// Возвращает контесты, где текущий пользователь является организатором
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> ListMyContests(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await contestService.GetForUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Возвращает контесты, где текущий пользователь участвует
    /// </summary>
    [Authorize]
    [HttpGet("participating")]
    public async Task<IActionResult> ListParticipating(
        [FromQuery] [Range(1, 100)] int pageSize = 10,
        [FromQuery] [Range(0, int.MaxValue)] int page = 0,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await contestService.GetParticipatingAsync(userId, pageSize, page, cancellationToken);
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

    /// <summary>
    /// Возвращает отправки текущего пользователя в рамках контеста
    /// </summary>
    [Authorize]
    [HttpGet("{contestId:int}/submissions/my")]
    public async Task<IActionResult> GetMyContestSubmissions([FromRoute] int contestId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await submitService.GetUserContestSubmissionsAsync(userId, contestId, cancellationToken);

        return result.Status switch
        {
            ContestSubmissionQueryStatus.Success => Ok(result.Submissions.Select(SubmissionResponse.FromEntity)),
            ContestSubmissionQueryStatus.ContestNotFound => NotFound("Contest not found."),
            ContestSubmissionQueryStatus.AccessDenied => Forbid(),
            _ => StatusCode(500, "Failed to load submissions.")
        };
    }

    /// <summary>
    /// Возвращает попытки текущего пользователя в контесте с результатами по задачам
    /// </summary>
    [Authorize]
    [HttpGet("{contestId:int}/attempts/my")]
    public async Task<IActionResult> GetMyContestAttempts([FromRoute] int contestId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await contestService.GetUserAttemptsAsync(contestId, userId, cancellationToken);
        return result.Status switch
        {
            ContestAttemptQueryStatus.Success => Ok(result.Attempts),
            ContestAttemptQueryStatus.ContestNotFound => NotFound("Contest not found."),
            ContestAttemptQueryStatus.AccessDenied => Forbid(),
            _ => StatusCode(500, "Failed to load attempts.")
        };
    }

    /// <summary>
    /// Возвращает участников контеста с пагинацией
    /// </summary>
    [HttpGet("{contestId:int}/members")]
    public async Task<IActionResult> GetContestMembers(
        [FromRoute] int contestId,
        [FromQuery] [Range(1, 100)] int pageSize = 25,
        [FromQuery] [Range(0, int.MaxValue)] int page = 0,
        CancellationToken cancellationToken = default)
    {
        var result = await contestService.GetMembersPageAsync(contestId, pageSize, page, cancellationToken);
        return result.Status switch
        {
            ContestMembersQueryStatus.Success => Ok(result.Page),
            ContestMembersQueryStatus.ContestNotFound => NotFound("Contest not found."),
            ContestMembersQueryStatus.InvalidPagination => BadRequest("Invalid pagination parameters."),
            _ => StatusCode(500, "Failed to load contest members.")
        };
    }

    /// <summary>
    /// Проверяет, зарегистрирован ли текущий пользователь на контесте
    /// </summary>
    [Authorize]
    [HttpGet("{contestId:int}/registered")]
    public async Task<IActionResult> IsCurrentUserRegistered([FromRoute] int contestId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var isRegistered = await contestService.IsUserRegisteredAsync(contestId, userId, cancellationToken);
        return Ok(new { isRegistered });
    }
}
