using LiquidCode.Extensions;
using LiquidCode.Models.Api.MissionsController;
using LiquidCode.Services.MissionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Controllers;

/// <summary>
/// Missions controller handling mission upload, retrieval, and management
/// </summary>
[Route("[controller]")]
[ApiController]
public class MissionsController(IMissionService missionService) : ControllerBase
{
    /// <summary>
    /// Uploads a new mission from a ZIP file
    /// </summary>
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMission([FromForm] UploadMissionForm form, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await missionService.UploadMissionAsync(form, userId, cancellationToken);
        if (result == null)
            return BadRequest("Mission upload failed. Ensure the ZIP file contains a valid 'statement-sections' folder.");

        return Ok(result);
    }

    /// <summary>
    /// Gets a public download link for a mission's statement files
    /// </summary>
    [HttpGet("get-mission-download-link")]
    public async Task<IActionResult> GetMissionDownloadLink([FromQuery] int id, CancellationToken cancellationToken)
    {
        var link = await missionService.GetMissionDownloadLinkAsync(id, cancellationToken);
        if (link == null)
            return NotFound("Mission not found.");

        return Ok(new { downloadUrl = link });
    }

    /// <summary>
    /// Gets mission text data in a specific language
    /// </summary>
    [HttpGet("get-mission-texts")]
    public async Task<IActionResult> GetMissionTexts([FromQuery] int id, [FromQuery] string language, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(language))
            return BadRequest("Language parameter is required.");

        var textData = await missionService.GetMissionTextAsync(id, language, cancellationToken);
        if (textData == null)
            return NotFound("Mission or language not found.");

        return Ok(textData);
    }

    /// <summary>
    /// Gets a paginated list of all missions
    /// </summary>
    [HttpGet("get-missions-list")]
    public async Task<IActionResult> GetMissionsList([FromQuery] int pageSize = 10, [FromQuery] int page = 0, CancellationToken cancellationToken = default)
    {
        var result = await missionService.GetMissionsListAsync(pageSize, page, cancellationToken);
        if (result == null)
            return BadRequest("Invalid pagination parameters.");

        return Ok(result);
    }
}