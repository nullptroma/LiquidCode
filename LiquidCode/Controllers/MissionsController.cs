using System.Security.Claims;
using LiquidCode.Db;
using LiquidCode.Models.Api.MissionsController;
using LiquidCode.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Controllers;

[Route("[controller]")]
[ApiController]
public class MissionsController(LiquidDbContext dbContext) : ControllerBase
{
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMission([FromForm] UploadMissionForm form)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();
        var user = await dbContext.Users.FindAsync(userId);
        if(user == null)
            return Unauthorized();
        
        var dbMission = new DbMission
        {
            Author = user,
            Name = form.Name,
            S3FileName = "Test random file name",
            Difficulty = form.Difficulty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Missions.Add(dbMission);
        await dbContext.SaveChangesAsync();
        return Ok();
    }
}