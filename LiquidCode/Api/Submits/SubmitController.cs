using System;
using System.Linq;
using LiquidCode.Api.Submits.Requests;
using LiquidCode.Api.Submits.Responses;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Api.Submits;

/// <summary>
/// Контроллер отправки, обрабатывающий отправку решений пользователей и результаты
/// </summary>
[Route("submits")]
[ApiController]
public class SubmitController(
    ISubmitService submitService,
    ILogger<SubmitController> logger) : ControllerBase
{
    private const string CallbackRouteName = "SubmitTesterCallback";
    private readonly ISubmitService _submitService = submitService;
    private readonly ILogger<SubmitController> _logger = logger;

    /// <summary>
    /// Отправляет решение для миссии
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SubmitSolution([FromBody] SubmitSolutionRequest request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        var solution = await _submitService.SubmitSolutionAsync(
            request.MissionId,
            userId,
            request.SourceCode,
            request.Language,
            request.LanguageVersion,
            request.ContestId,
            cancellationToken);

        if (solution == null)
            return BadRequest("Solution submission failed. Mission may not exist or language is not supported.");

        try
        {
            var callbackToken = solution.CallbackToken;
            if (string.IsNullOrWhiteSpace(callbackToken))
                throw new InvalidOperationException("Callback token is not generated.");

            var callbackUrl = BuildCallbackUrl(callbackToken);

            var dispatchResult = await _submitService.DispatchSolutionAsync(solution, callbackUrl, cancellationToken);
            if (!dispatchResult.Success)
                return StatusCode(StatusCodes.Status502BadGateway, dispatchResult.ErrorMessage ?? "Failed to dispatch solution to testing module.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch solution {SolutionId} to testing module", solution.Id);
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to dispatch solution to testing module.");
        }

        return Ok(SolutionResponse.FromEntity(solution));
    }

    /// <summary>
    /// Получает все отправки текущего пользователя
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMySubmissions(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var submissions = await _submitService.GetUserSubmissionsAsync(userId, cancellationToken);

        var result = submissions.Select(SubmissionResponse.FromEntity);

        return Ok(result);
    }

    /// <summary>
    /// Получает конкретную отправку пользователя по ID
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubmissionById([FromRoute] int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var submission = await _submitService.GetSubmissionAsync(id, cancellationToken);

        if (submission == null || submission.User.Id != userId)
            return NotFound("Submission not found or access denied.");

        return Ok(SubmissionResponse.FromEntity(submission));
    }

    /// <summary>
    /// Получает все отправки текущего пользователя для конкретной миссии
    /// </summary>
    [Authorize]
    [HttpGet("my/mission/{missionId}")]
    public async Task<IActionResult> GetMyMissionSubmissions([FromRoute] int missionId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var submissions = await _submitService.GetUserSubmissionsAsync(userId, cancellationToken);

        var filtered = submissions
            .Where(sub => sub.Solution.Mission.Id == missionId)
            .Select(SubmissionResponse.FromEntity)
            .ToList();

        return Ok(filtered);
    }

    /// <summary>
    /// Получает обновление статуса решения от тестирующего модуля
    /// </summary>
    [HttpPost("testing/callback/{token}", Name = CallbackRouteName)]
    public async Task<IActionResult> ReceiveTesterCallback([FromRoute] string token, [FromBody] TesterCallbackRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Callback token is required.");

        if (request.SubmitId <= 0 || request.SubmitId > int.MaxValue)
            return BadRequest("SubmitId value is out of supported range.");

        var updateResult = await _submitService.UpdateTesterStatusAsync(
            (int)request.SubmitId,
            token,
            request.State,
            request.ErrorCode,
            request.Message,
            request.CurrentTest,
            request.AmountOfTests,
            cancellationToken);

        return updateResult.Status switch
        {
            TesterCallbackUpdateStatus.Success when updateResult.Solution != null => Accepted(SolutionResponse.FromEntity(updateResult.Solution)),
            TesterCallbackUpdateStatus.NotFound => NotFound("Solution not found."),
            TesterCallbackUpdateStatus.TokenMismatch => StatusCode(StatusCodes.Status403Forbidden, "Invalid or expired callback token."),
            TesterCallbackUpdateStatus.Error => StatusCode(StatusCodes.Status500InternalServerError, "Failed to update solution status."),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unexpected tester callback processing result.")
        };
    }
    private string BuildCallbackUrl(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Callback token is not provided.");

        if (!Request.Host.HasValue)
            throw new InvalidOperationException("Unable to determine request host for callback URL generation.");

        var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? Uri.UriSchemeHttps : Request.Scheme;

        var link = Url.RouteUrl(CallbackRouteName, values: new { token }, protocol: scheme, host: Request.Host.Value);
        if (string.IsNullOrWhiteSpace(link))
            throw new InvalidOperationException("Unable to build callback URL.");

        return link;
    }
}
