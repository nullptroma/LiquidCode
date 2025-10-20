using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using LiquidCode.Api.Missions.Requests;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Shared.Constants;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.External.S3;

namespace LiquidCode.Domain.Services.Missions;

/// <summary>
/// Service implementation for mission-related operations
/// </summary>
public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IS3BucketClient _s3Client;
    private readonly IS3PublicBucketClient _s3PublicClient;
    private readonly ILogger<MissionService> _logger;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    public MissionService(
        IMissionRepository missionRepository,
        IS3BucketClient s3Client,
        IS3PublicBucketClient s3PublicClient,
        ILogger<MissionService> logger)
    {
        _missionRepository = missionRepository;
        _s3Client = s3Client;
        _s3PublicClient = s3PublicClient;
        _logger = logger;
    }

    public async Task<MissionResponse?> UploadMissionAsync(UploadMissionRequest form, int userId, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var unpackFolder = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        var packageZipPath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip");
        var statementsZipPath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip");

        try
        {
            // Save uploaded file
            _logger.LogInformation("Saving mission file: {FileName}", form.MissionFile.Name);
            using (var fileStream = System.IO.File.Open(packageZipPath, FileMode.OpenOrCreate))
            {
                await form.MissionFile.CopyToAsync(fileStream, cancellationToken);
            }

            // Extract ZIP file
            _logger.LogInformation("Extracting mission ZIP to: {UnpackFolder}", unpackFolder);
            ZipFile.ExtractToDirectory(packageZipPath, unpackFolder);

            // Verify statement-sections folder exists
            var statementSectionsPath = Path.Combine(unpackFolder, MissionStatementPaths.StatementSectionsFolder);
            if (!Directory.Exists(statementSectionsPath))
            {
                _logger.LogError("statement-sections folder not found in mission ZIP");
                return null;
            }

            // Pack statement sections
            _logger.LogInformation("Creating statements ZIP: {StatementsZipPath}", statementsZipPath);
            ZipFile.CreateFromDirectory(statementSectionsPath, statementsZipPath, CompressionLevel.SmallestSize, false);

            // Upload to S3
            _logger.LogInformation("Uploading mission files to S3");
            var privateKey = await _s3Client.UploadFileWithRandomKey(S3BucketKeys.PrivateProblems, packageZipPath);
            var publicKey = await _s3PublicClient.UploadFileWithRandomKey(S3BucketKeys.PublicProblems, statementsZipPath);

            // Create mission in database
            var dbMission = new DbMission
            {
                Author = new DbUser { Id = userId },
                Name = form.Name,
                S3PrivateKey = privateKey,
                S3PublicKey = publicKey,
                Difficulty = form.Difficulty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _missionRepository.AddAsync(dbMission, cancellationToken);

            // Parse and store mission text data
            var missionTexts = ExtractMissionTexts(statementSectionsPath, dbMission.Id);
            
            // Update mission name from Russian if available, otherwise from first available language
            var russianText = missionTexts.FirstOrDefault(t => t.Language == "russian");
            if (russianText != null)
            {
                var russianData = JsonSerializer.Deserialize<JsonMissionData>(russianText.Data, JsonSerializerOptions);
                if (russianData?.Name != null)
                    dbMission.Name = russianData.Name;
            }
            else if (missionTexts.Count > 0)
            {
                var firstData = JsonSerializer.Deserialize<JsonMissionData>(missionTexts[0].Data, JsonSerializerOptions);
                if (firstData?.Name != null)
                    dbMission.Name = firstData.Name;
            }

            // Add mission texts to database
            await _missionRepository.AddMissionTextsAsync(missionTexts, cancellationToken);
            await _missionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Mission uploaded successfully: {MissionId}", dbMission.Id);
            return MissionResponse.FromEntity(dbMission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading mission");
            return null;
        }
        finally
        {
            // Cleanup temporary files
            CleanupTemporaryFiles(unpackFolder, packageZipPath, statementsZipPath);
        }
    }

    public async Task<string?> GetMissionDownloadLinkAsync(int missionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var mission = await _missionRepository.FindByIdAsync(missionId, cancellationToken);
            if (mission == null)
            {
                _logger.LogWarning("Mission not found: {MissionId}", missionId);
                return null;
            }

            return _s3PublicClient.GetPublicDownloadUrl(mission.S3PublicKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mission download link: {MissionId}", missionId);
            return null;
        }
    }

    public async Task<string?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default)
    {
        try
        {
            var mission = await _missionRepository.FindByIdAsync(missionId, cancellationToken);
            if (mission == null)
            {
                _logger.LogWarning("Mission not found: {MissionId}", missionId);
                return null;
            }

            var textData = await _missionRepository.GetMissionTextAsync(missionId, language, cancellationToken);
            if (textData == null)
            {
                _logger.LogWarning("Mission text not found: {MissionId}, {Language}", missionId, language);
                return null;
            }

            return textData.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mission text: {MissionId}, {Language}", missionId, language);
            return null;
        }
    }

    public async Task<MissionsPageResponse?> GetMissionsListAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageSize <= 0 || pageNumber < 0)
            {
                _logger.LogWarning("Invalid pagination parameters: pageSize={PageSize}, pageNumber={PageNumber}", pageSize, pageNumber);
                return null;
            }

            var (missions, hasNextPage) = await _missionRepository.GetMissionsPageAsync(pageSize, pageNumber, cancellationToken);
            var apiList = missions.Select(MissionResponse.FromEntity);

            return new MissionsPageResponse(hasNextPage, apiList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting missions list");
            return null;
        }
    }

    private List<DbMissionPublicTextData> ExtractMissionTexts(string statementSectionsPath, int missionId)
    {
        var missionTexts = new List<DbMissionPublicTextData>();
        var directoryInfo = new DirectoryInfo(statementSectionsPath);

        foreach (var languageDir in directoryInfo.GetDirectories())
        {
            try
            {
                var data = GetDataFromStatementSections(languageDir);
                var json = JsonSerializer.Serialize(data, JsonSerializerOptions);

                missionTexts.Add(new DbMissionPublicTextData
                {
                    MissionId = missionId,
                    Language = languageDir.Name,
                    Data = json
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting mission text for language: {Language}", languageDir.Name);
            }
        }

        return missionTexts;
    }

    private JsonMissionData GetDataFromStatementSections(DirectoryInfo dir)
    {
        var files = dir.GetFiles();
        var data = new JsonMissionData
        {
            Name = System.IO.File.ReadAllText(files.Single(f => f.Name == MissionStatementPaths.NameFile).FullName),
            Input = System.IO.File.ReadAllText(files.Single(f => f.Name == MissionStatementPaths.InputFile).FullName),
            Output = System.IO.File.ReadAllText(files.Single(f => f.Name == MissionStatementPaths.OutputFile).FullName),
            Legend = System.IO.File.ReadAllText(files.Single(f => f.Name == MissionStatementPaths.LegendFile).FullName),
            Examples = [],
            ExampleAnswers = []
        };

        var exampleFiles = dir.GetFiles()
            .Where(f => f.Name.StartsWith(MissionStatementPaths.ExampleFilePrefix))
            .OrderBy(f =>
            {
                var numberPart = f.Name[MissionStatementPaths.ExampleFilePrefix.Length..];
                if (numberPart.Contains('.'))
                    numberPart = numberPart[..numberPart.IndexOf(".", StringComparison.Ordinal)];
                return int.TryParse(numberPart, out var num) ? num : int.MaxValue;
            });

        foreach (var exampleFile in exampleFiles)
        {
            var content = System.IO.File.ReadAllText(exampleFile.FullName);
            if (exampleFile.Name.EndsWith("a"))
                data.ExampleAnswers.Add(content);
            else
                data.Examples.Add(content);
        }

        return data;
    }

    private void CleanupTemporaryFiles(string unpackFolder, string packageZipPath, string statementsZipPath)
    {
        try
        {
            if (Directory.Exists(unpackFolder))
                Directory.Delete(unpackFolder, true);
            if (System.IO.File.Exists(packageZipPath))
                System.IO.File.Delete(packageZipPath);
            if (System.IO.File.Exists(statementsZipPath))
                System.IO.File.Delete(statementsZipPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up temporary files");
        }
    }
}

/// <summary>
/// Internal model for mission statement data structure
/// </summary>
internal class JsonMissionData
{
    public string Name { get; set; } = "";
    public string Input { get; set; } = "";
    public string Output { get; set; } = "";
    public string Legend { get; set; } = "";
    public List<string> Examples { get; set; } = [];
    public List<string> ExampleAnswers { get; set; } = [];
}
