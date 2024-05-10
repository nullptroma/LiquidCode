using System.Security.Claims;
using LiquidCode.Db;
using LiquidCode.Models.Api.SubmitController;
using LiquidCode.Models.Database;
using LiquidCode.Services.TestingModuleHttpClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Controllers;

[Route("[controller]")]
public class SubmitController(LiquidDbContext dbContext, TestingHttpClient testingClient) : ControllerBase
{
    [Authorize]
    [HttpPost("user-submit")]
    public async Task<IActionResult> SubmitFromUser([FromBody] SolutionSubmitModel model)
    {
        var mission = await dbContext.Missions.FindAsync(model.MissionId);
        if (mission == null)
            return NotFound("Mission not found");
        if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized("User not found");
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User not found");

        var dbSolution = new DbSolution
        {
            Id = 0,
            Mission = mission,
            Language = model.Language,
            LanguageVersion = model.LanguageVersion,
            SourceCode = model.SourceCode,
            Status = "",
            Time = DateTime.UtcNow
        };
        var dbUserSubmit = new DbUserSubmit
        {
            Id = 0,
            User = user,
            Solution = dbSolution
        };
        dbContext.Solutions.Add(dbSolution);
        dbContext.UserSubmits.Add(dbUserSubmit);
        await dbContext.SaveChangesAsync();

        await testingClient.PostData(dbSolution.Id, 123, dbSolution.SourceCode, "cpp");
        Console.WriteLine($"Sent: {dbSolution.Id}");
        return Ok(new UserSubmitInfoModel(dbUserSubmit.Id, userId, new SolutionInfoModel(dbSolution.Mission.Id,
            dbSolution.Language,
            dbSolution.LanguageVersion,
            dbSolution.SourceCode,
            dbSolution.Status,
            dbSolution.Time)));
    }

    [Authorize]
    [HttpGet("get-all-user-submits")]
    public IActionResult GetAllUserSubmits()
    {
        if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized("User not found");
        var solutions = dbContext.UserSubmits
            .Include(sub => sub.Solution.Mission)
            .Where(sub => sub.User.Id == userId)
            .Select(sub => new UserSubmitInfoModel(sub.Id, userId, new SolutionInfoModel(sub.Solution.Mission.Id,
                sub.Solution.Language,
                sub.Solution.LanguageVersion,
                sub.Solution.SourceCode,
                sub.Solution.Status,
                sub.Solution.Time)));
        return Ok(solutions);
    }

    [Authorize]
    [HttpGet("get-user-submit-by-id")]
    public async Task<IActionResult> GetUserSubmitById(int submitId)
    {
        if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized("User not found");
        var userSubmit = await dbContext.UserSubmits.Include(s => s.Solution).Include(s => s.Solution.Mission)
            .SingleOrDefaultAsync(s => s.Id == submitId && s.User.Id == userId);
        if (userSubmit == null)
            return NotFound("Submit not found");
        var dbSolution = userSubmit.Solution;
        var solution = new SolutionInfoModel(dbSolution.Mission.Id,
            dbSolution.Language,
            dbSolution.LanguageVersion,
            dbSolution.SourceCode,
            dbSolution.Status,
            dbSolution.Time);
        return Ok(new UserSubmitInfoModel(userSubmit.Id, userId, solution));
    }

    [Authorize]
    [HttpGet("get-user-mission-submits-by-id")]
    public IActionResult GetMissionSubmits(int missionId)
    {
        if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized("User not found");
        var submits = dbContext.UserSubmits
            .Where(sub => sub.User.Id == userId && sub.Solution.Mission.Id == missionId)
            .Select(sub => new UserSubmitInfoModel(sub.Id, sub.User.Id, new SolutionInfoModel(sub.Solution.Mission.Id,
                sub.Solution.Language,
                sub.Solution.LanguageVersion,
                sub.Solution.SourceCode,
                sub.Solution.Status,
                sub.Solution.Time)));
        return Ok(submits);
    }

    // TODO remove trash
    private static readonly string[] VerdictStatusCode =
    [
        "Accepted", "Wrong answer", "Time limit", "Memory limit", "Internal error", "Runtime error", "Compilation error"
    ];

    [HttpPost("update-solution-status")]
    public async Task<IActionResult> UpdateSolutionStatus([FromBody] UpdateSolutionStatusModel status)
    {
        var verdict = status.VerdictCode == -1 ? "Running" : VerdictStatusCode[status.VerdictCode];
        Console.WriteLine($"Sol: {status} with verdict: {verdict}");
        var newStatus = verdict;
        switch (status.VerdictCode)
        {
            case -1:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                newStatus += " #"+status.TestCase;
                break;
        }

        var solution = dbContext.Solutions.SingleOrDefault(sol => sol.Id == status.SubmissionId);
        if (solution == null)
            return NotFound();
        solution.Status = newStatus;
        dbContext.Solutions.Update(solution);
        await dbContext.SaveChangesAsync();
        return Ok();
    }
}