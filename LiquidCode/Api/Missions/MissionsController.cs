using System.ComponentModel.DataAnnotations;
using LiquidCode.Api.Missions.Requests;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Missions;

/// <summary>
/// Контроллер миссий, обрабатывающий загрузку миссий, получение и управление
/// </summary>
[Route("missions")]
[ApiController]
public class MissionsController(IMissionService missionService) : ControllerBase
{
    /// <summary>
    /// Загружает новую миссию из ZIP файла
    /// </summary>
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMission([FromForm] UploadMissionRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await missionService.UploadMissionAsync(request, userId, cancellationToken);
        if (result == null)
            return BadRequest("Mission upload failed. Ensure the ZIP file contains a valid 'statements' folder.");

        return Ok(result);
    }

    /// <summary>
    /// Получает подробную информацию о миссии с текстами и медиа
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMission([FromRoute] int id, CancellationToken cancellationToken)
    {
        var mission = await missionService.GetMissionAsync(id, cancellationToken);
        if (mission == null)
            return NotFound("Mission not found.");

        return Ok(mission);
    }

    /// <summary>
    /// Получает постраничный список всех миссий
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMissionsList(
        [FromQuery] [Range(1, 100)] int pageSize = 10,
        [FromQuery] [Range(0, int.MaxValue)] int page = 0,
        [FromQuery] List<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var result = await missionService.GetMissionsListAsync(pageSize, page, tags, cancellationToken);
        if (result == null)
            return BadRequest("Invalid pagination parameters.");

        return Ok(result);
    }

    /// <summary>
    /// Возвращает миссии, загруженные текущим пользователем
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyMissions(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var result = await missionService.GetMyMissionsAsync(userId, cancellationToken);
        return Ok(result);
    }
}
