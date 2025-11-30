using System.ComponentModel.DataAnnotations;
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
    /// Возвращает общую информацию о профиле пользователя
    /// </summary>
    [HttpGet("{username}")]
    public async Task<IActionResult> GetOverview([FromRoute] string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username is required.");

        var requesterId = User.TryGetUserId(out var userId) ? userId : (int?)null;
        var overview = await profileService.GetOverviewAsync(username, requesterId, cancellationToken);
        return overview == null ? NotFound() : Ok(overview);
    }

    /// <summary>
    /// Возвращает статистику по задачам пользователя
    /// </summary>
    [HttpGet("{username}/missions")]
    public async Task<IActionResult> GetMissions(
        [FromRoute] string username,
        [FromQuery] [Range(0, int.MaxValue)] int recentPage = 0,
        [FromQuery] [Range(1, 15)] int recentPageSize = 15,
        [FromQuery] [Range(0, int.MaxValue)] int authoredPage = 0,
        [FromQuery] [Range(1, 100)] int authoredPageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username is required.");

        var requesterId = User.TryGetUserId(out var userId) ? userId : (int?)null;
        var query = new ProfileProblemsQuery(recentPage, recentPageSize, authoredPage, authoredPageSize);
        var response = await profileService.GetProblemsAsync(username, requesterId, query, cancellationToken);
        return response == null ? NotFound() : Ok(response);
    }

    /// <summary>
    /// Возвращает статьи пользователя
    /// </summary>
    [HttpGet("{username}/articles")]
    public async Task<IActionResult> GetArticles(
        [FromRoute] string username,
        [FromQuery] [Range(0, int.MaxValue)] int page = 0,
        [FromQuery] [Range(1, 100)] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username is required.");

        var requesterId = User.TryGetUserId(out var userId) ? userId : (int?)null;
        var query = new ProfileArticlesQuery(page, pageSize);
        var response = await profileService.GetArticlesAsync(username, requesterId, query, cancellationToken);
        return response == null ? NotFound() : Ok(response);
    }

    /// <summary>
    /// Возвращает контесты пользователя
    /// </summary>
    [HttpGet("{username}/contests")]
    public async Task<IActionResult> GetContests(
        [FromRoute] string username,
        [FromQuery] [Range(0, int.MaxValue)] int upcomingPage = 0,
        [FromQuery] [Range(1, 100)] int upcomingPageSize = 10,
        [FromQuery] [Range(0, int.MaxValue)] int pastPage = 0,
        [FromQuery] [Range(1, 100)] int pastPageSize = 10,
        [FromQuery] [Range(0, int.MaxValue)] int minePage = 0,
        [FromQuery] [Range(1, 100)] int minePageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username is required.");

        var requesterId = User.TryGetUserId(out var userId) ? userId : (int?)null;
        var query = new ProfileContestsQuery(
            upcomingPage,
            upcomingPageSize,
            pastPage,
            pastPageSize,
            minePage,
            minePageSize);

        var response = await profileService.GetContestsAsync(username, requesterId, query, cancellationToken);
        return response == null ? NotFound() : Ok(response);
    }
}
