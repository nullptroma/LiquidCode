using LiquidCode.Extensions;
using LiquidCode.Models.Api.SubmitController;
using LiquidCode.Services.SubmitService;
using LiquidCode.Services.TestingModuleHttpClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Controllers;

/// <summary>
/// Submit controller handling user solution submissions and results
/// </summary>
[Route("[controller]")]
[ApiController]
public class SubmitController(ISubmitService submitService, TestingHttpClient testingClient) : ControllerBase
{
    /// <summary>
    /// Submits a solution for a mission
    /// </summary>
    [Authorize]
    [HttpPost("user-submit")]
    public async Task<IActionResult> SubmitFromUser([FromBody] SolutionSubmitModel model, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var solution = await submitService.SubmitSolutionAsync(
            model.MissionId, userId, model.SourceCode, model.Language, model.LanguageVersion, cancellationToken);

        if (solution == null)
            return BadRequest("Solution submission failed. Mission may not exist or language is not supported.");

        // Send to testing module asynchronously (fire and forget)
        _ = testingClient.PostData(solution.Id, model.MissionId, model.SourceCode, model.Language);

        return Ok(new UserSubmitInfoModel(
            solution.Id,
            userId,
            new SolutionInfoModel(
                model.MissionId,
                solution.Language,
                solution.LanguageVersion,
                solution.SourceCode,
                solution.Status,
                solution.Time)));
    }

    /// <summary>
    /// Gets all submissions by the current user
    /// </summary>
    [Authorize]
    [HttpGet("get-all-user-submits")]
    public async Task<IActionResult> GetAllUserSubmits(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var submissions = await submitService.GetUserSubmissionsAsync(userId, cancellationToken);

        var result = submissions.Select(sub => new UserSubmitInfoModel(
            sub.Id,
            userId,
            new SolutionInfoModel(
                sub.Solution.Mission.Id,
                sub.Solution.Language,
                sub.Solution.LanguageVersion,
                sub.Solution.SourceCode,
                sub.Solution.Status,
                sub.Solution.Time)));

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific user submission by ID
    /// </summary>
    [Authorize]
    [HttpGet("get-user-submit-by-id")]
    public async Task<IActionResult> GetUserSubmitById([FromQuery] int submitId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var submission = await submitService.GetSubmissionAsync(submitId, cancellationToken);

        if (submission == null || submission.User.Id != userId)
            return NotFound("Submission not found or access denied.");

        return Ok(new UserSubmitInfoModel(
            submission.Id,
            userId,
            new SolutionInfoModel(
                submission.Solution.Mission.Id,
                submission.Solution.Language,
                submission.Solution.LanguageVersion,
                submission.Solution.SourceCode,
                submission.Solution.Status,
                submission.Solution.Time)));
    }

    /// <summary>
    /// Gets all submissions by the current user for a specific mission
    /// </summary>
    [Authorize]
    [HttpGet("get-user-mission-submits-by-id")]
    public async Task<IActionResult> GetMissionSubmits([FromQuery] int missionId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized("User ID not found in claims.");

        var submissions = await submitService.GetUserSubmissionsAsync(userId, cancellationToken);

        var filtered = submissions
            .Where(sub => sub.Solution.Mission.Id == missionId)
            .Select(sub => new UserSubmitInfoModel(
                sub.Id,
                userId,
                new SolutionInfoModel(
                    sub.Solution.Mission.Id,
                    sub.Solution.Language,
                    sub.Solution.LanguageVersion,
                    sub.Solution.SourceCode,
                    sub.Solution.Status,
                    sub.Solution.Time)))
            .ToList();

        return Ok(filtered);
    }

    /// <summary>
    /// Updates solution status (called by testing module)
    /// </summary>
    [HttpPost("update-solution-status")]
    public async Task<IActionResult> UpdateSolutionStatus([FromBody] UpdateSolutionStatusModel status, CancellationToken cancellationToken)
    {
        if (status == null || status.SubmissionId <= 0)
            return BadRequest("Invalid submission ID.");

        var verdictMessage = FormatVerdictMessage(status.VerdictCode, status.TestCase);

        var result = await submitService.UpdateSolutionStatusAsync(status.SubmissionId, verdictMessage, cancellationToken);
        if (result == null)
            return NotFound("Solution not found.");

        return Accepted();
    }

    /// <summary>
    /// Formats verdict message for solution status
    /// </summary>
    private string FormatVerdictMessage(int verdictCode, int? testCase)
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