using System.IO.Compression;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using LiquidCode.Db;
using LiquidCode.Models.Api.MissionsController;
using LiquidCode.Models.Database;
using LiquidCode.Services.S3ClientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Controllers;

[Route("[controller]")]
[ApiController]
public class MissionsController(
    LiquidDbContext dbContext,
    ILogger<MissionsController> logger,
    IS3BucketClient s3Client,
    IS3PublicBucketClient s3PublicClient) : ControllerBase
{
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMission([FromForm] UploadMissionForm form)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized();

        var tempDir = Path.GetTempPath();
        var unpackFolder = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        var packageZipPath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip");
        var statementsZipPath =
            Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip");

        var statementSectionsPath = Path.Combine(unpackFolder, "statement-sections");
        try
        {
            logger.LogInformation("Saving {fileName} as {dest}", form.MissionFile.Name, packageZipPath);
            var packageZipFileStream = System.IO.File.Open(packageZipPath, FileMode.OpenOrCreate);
            await form.MissionFile.CopyToAsync(packageZipFileStream);
            packageZipFileStream.Close();

            logger.LogInformation("Unpacking {fileName} into {dest}..", packageZipPath, unpackFolder);
            ZipFile.ExtractToDirectory(packageZipPath, unpackFolder);

            logger.LogInformation("Search statement-sections folder in {dest}..", unpackFolder);
            if (!Directory.Exists(statementSectionsPath))
                return BadRequest();

            logger.LogInformation("Packing statement-sections into {dest}..", statementsZipPath);
            ZipFile.CreateFromDirectory(statementSectionsPath, statementsZipPath, CompressionLevel.SmallestSize, false);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        DbMission dbMission;
        try
        {
            var privateKey = await s3Client.UploadFileWithRandomKey("problems", packageZipPath);
            var publicKey = await s3PublicClient.UploadFileWithRandomKey("problems-public", statementsZipPath);

            dbMission = new DbMission
            {
                Author = user,
                Name = form.Name,
                S3PrivateKey = privateKey,
                S3PublicKey = publicKey,
                Difficulty = form.Difficulty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbMission = dbContext.Missions.Add(dbMission).Entity;
            await dbContext.SaveChangesAsync();

            List<DbMissionPublicTextData> missionTexts = [];
            foreach (var dir in new DirectoryInfo(statementSectionsPath).GetDirectories())
            {
                missionTexts.Add(new DbMissionPublicTextData
                {
                    MissionId = dbMission.Id,
                    Language = dir.Name,
                    Data = CreateJsonFromStatementSections(dir)
                });
            }

            dbContext.MissionsTextData.AddRange(missionTexts);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            return BadRequest();
        }
        finally
        {
            // cleanup tmp
            if (Directory.Exists(unpackFolder))
                Directory.Delete(unpackFolder, true);
            if (System.IO.File.Exists(packageZipPath))
                System.IO.File.Delete(packageZipPath);
            if (System.IO.File.Exists(statementsZipPath))
                System.IO.File.Delete(statementsZipPath);
        }

        return Ok(new MissionModel(dbMission));
    }

    [HttpGet]
    [Route("get-mission-download-link")]
    public IActionResult GetMission([FromQuery] int id)
    {
        var mission = dbContext.Missions.Find(id);
        if (mission == null)
            return NotFound();
        return Ok(s3PublicClient.GetPublicDownloadUrl(mission.S3PublicKey));
    }

    [HttpGet]
    [Route("get-mission-texts")]
    public IActionResult GetMissionTexts([FromQuery] int id, [FromQuery] string language)
    {
        var mission = dbContext.Missions.Find(id);
        if (mission == null)
            return NotFound();
        var texts = dbContext.MissionsTextData.SingleOrDefault(m => m.MissionId == id && m.Language == language);
        if (texts == null)
            return NotFound();
        return Ok(texts.Data);
    }

    [HttpGet]
    [Route("get-missions-list")]
    public IActionResult GetMissionsList([FromQuery] int pageSize, [FromQuery] int page)
    {
        if (pageSize <= 0 || page < 0)
            return BadRequest();
        var hasNext = dbContext.Missions.Count() > pageSize * (page + 1);
        var missions = dbContext.Missions.OrderBy(m=>m.Id).Skip(pageSize * page).Take(pageSize);
        var apiList = missions.Select(m =>
            new MissionModel(m));
        return Ok(new MissionsPage(hasNext, apiList));
    }

    // TODO remove
    private string CreateJsonFromStatementSections(DirectoryInfo dir)
    {
        JsonMissionData data = new()
        {
            Name = System.IO.File.ReadAllText(dir.GetFiles().Single(fi => fi.Name == "name.tex").FullName),
            Input = System.IO.File.ReadAllText(dir.GetFiles().Single(fi => fi.Name == "input.tex").FullName),
            Output = System.IO.File.ReadAllText(dir.GetFiles().Single(fi => fi.Name == "output.tex").FullName),
            Legend = System.IO.File.ReadAllText(dir.GetFiles().Single(fi => fi.Name == "legend.tex").FullName),
            Examples = [],
            ExampleAnswers = []
        };

        foreach (var exampleFile in dir.GetFiles().Where(fi => fi.Name.StartsWith("example"))
                     .OrderBy(fi =>
                     {
                         var number = string.Join("", fi.Name.Skip("example.".Length));
                         if (number.Contains('.'))
                             number = number[..number.IndexOf(".", StringComparison.Ordinal)];
                         return int.Parse(number);
                     }))
        {
            if (exampleFile.Name.EndsWith("a"))
                data.ExampleAnswers.Add(System.IO.File.ReadAllText(exampleFile.FullName));
            else
                data.Examples.Add(System.IO.File.ReadAllText(exampleFile.FullName));
        }

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };
        return JsonSerializer.Serialize(data, options);
    }

    class JsonMissionData
    {
        public string Name { get; set; } = "";
        public string Input { get; set; } = "";
        public string Output { get; set; } = "";
        public string Legend { get; set; } = "";
        public List<string> Examples { get; init; } = [];
        public List<string> ExampleAnswers { get; init; } = [];
    }
}