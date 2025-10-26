using LiquidCode.Api.Submits.Requests;
using LiquidCode.Api.Submits.Responses;
using LiquidCode.Domain.Services.Submits;
using LiquidCode.Infrastructure.External.TestingModule;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Submits;

/// <summary>
/// Контроллер отправки, обрабатывающий отправку решений пользователей и результаты
/// </summary>
[Route("submits")]
[ApiController]
public class SubmitController(ISubmitService submitService, TestingHttpClient testingClient) : ControllerBase
{
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

        var solution = await submitService.SubmitSolutionAsync(
            request.MissionId,
            userId,
            request.SourceCode,
            request.Language,
            request.LanguageVersion,
            request.ContestId,
            request.SourceType,
            cancellationToken);

        if (solution == null)
            return BadRequest("Solution submission failed. Mission may not exist or language is not supported.");

        // Отправить в модуль тестирования асинхронно (запустить и забыть)
        _ = testingClient.PostData(solution.Id, request.MissionId, request.SourceCode, request.Language);

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

        var submissions = await submitService.GetUserSubmissionsAsync(userId, cancellationToken);

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

        var submission = await submitService.GetSubmissionAsync(id, cancellationToken);

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

        var submissions = await submitService.GetUserSubmissionsAsync(userId, cancellationToken);

        var filtered = submissions
            .Where(sub => sub.Solution.Mission.Id == missionId)
            .Select(SubmissionResponse.FromEntity)
            .ToList();

        return Ok(filtered);
    }

    /// <summary>
    /// Обновляет статус решения (вызывается модулем тестирования)
    /// </summary>
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateSolutionStatus([FromBody] UpdateSolutionStatusRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var verdictMessage = FormatVerdictMessage(request.VerdictCode, request.TestCase);

        var result = await submitService.UpdateSolutionStatusAsync(request.SubmissionId, verdictMessage, cancellationToken);
        if (result == null)
            return NotFound("Solution not found.");

        return Accepted(SolutionResponse.FromEntity(result));
    }

    /// <summary>
    /// Форматирует сообщение вердикта для статуса решения
    /// </summary>
    private static string FormatVerdictMessage(int verdictCode, int? testCase)
    {
        var verdictMessages = new[]
        {
            "Accepted",
            "Wrong answer",
            "Time limit",
            "Memory limit",
            "Internal error",
            "Runtime error",
            "Compilation error"
        };

        if (verdictCode == -1)
            return "Running";

        var message = verdictCode >= 0 && verdictCode < verdictMessages.Length
            ? verdictMessages[verdictCode]
            : "Unknown verdict";

        if (testCase.HasValue && verdictCode >= 1 && verdictCode <= 5)
            message += $" #{testCase}";

        return message;
    }
}
