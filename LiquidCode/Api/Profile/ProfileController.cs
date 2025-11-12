using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Profile;

/// <summary>
/// Контроллер профиля пользователя
/// </summary>
[Route("profile")]
[ApiController]
public class ProfileController(IProfileService profileService) : ControllerBase
{
    /// <summary>
    /// Возвращает подробную информацию о профиле пользователя
    /// </summary>
    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile([FromRoute] string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username is required.");

        var requesterId = User.TryGetUserId(out var userId) ? userId : (int?)null;
        var profile = await profileService.GetProfileAsync(username, requesterId, cancellationToken);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }
}
