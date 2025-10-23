using LiquidCode.Api.Missions.Requests;
using LiquidCode.Domain.Services.Missions;
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
            return BadRequest("Mission upload failed. Ensure the ZIP file contains a valid 'statement-sections' folder.");

        return Ok(result);
    }

    /// <summary>
    /// Получает текстовые данные миссии на определенном языке
    /// </summary>
    [HttpGet("{id}/texts/{language}")]
    public async Task<IActionResult> GetMissionTexts([FromRoute] int id, [FromRoute] string language, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(language))
            return BadRequest("Language parameter is required.");

        var textData = await missionService.GetMissionTextAsync(id, language, cancellationToken);
        if (textData == null)
            return NotFound("Mission or language not found.");

        return Ok(textData);
    }

    /// <summary>
    /// Получает постраничный список всех миссий
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMissionsList([FromQuery] int pageSize = 10, [FromQuery] int page = 0, CancellationToken cancellationToken = default)
    {
        var result = await missionService.GetMissionsListAsync(pageSize, page, cancellationToken);
        if (result == null)
            return BadRequest("Invalid pagination parameters.");

        return Ok(result);
    }
}
