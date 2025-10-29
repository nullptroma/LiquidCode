using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using LiquidCode.Api.Missions.Requests;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Shared.Constants;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.External.S3;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Domain.Enums;

namespace LiquidCode.Domain.Services.Missions;

/// <summary>
/// Реализация сервиса для операций, связанных с миссиями
/// </summary>
public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IS3BucketClient _s3Client;
    private readonly IMediaService _mediaService;
    private readonly ILogger<MissionService> _logger;
    private readonly MissionArchiveProcessor _archiveProcessor;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    public MissionService(
        IMissionRepository missionRepository,
        IUserRepository userRepository,
        ITagRepository tagRepository,
        IS3BucketClient s3Client,
        IMediaService mediaService,
        ILogger<MissionService> logger,
        ILoggerFactory loggerFactory)
    {
        _missionRepository = missionRepository;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
        _s3Client = s3Client;
        _mediaService = mediaService;
        _logger = logger;
        _archiveProcessor = new MissionArchiveProcessor(loggerFactory.CreateLogger<MissionArchiveProcessor>(), mediaService);
    }

    public async Task<MissionResponse?> UploadMissionAsync(UploadMissionRequest form, int userId, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.GetTempPath();
        var unpackFolder = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        var packageZipPath = Path.Combine(tempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".zip");

        try
        {
            // Получить юзера
            var existingUser = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (existingUser == null)
            {
                _logger.LogError("User not found: {UserId}", userId);
                return null;
            }

            // Сохранить загруженный файл
            _logger.LogInformation("Saving mission file: {FileName}", form.MissionFile.Name);
            using (var fileStream = System.IO.File.Open(packageZipPath, FileMode.OpenOrCreate))
            {
                await form.MissionFile.CopyToAsync(fileStream, cancellationToken);
            }

            // Загрузить на S3
            _logger.LogInformation("Uploading mission files to S3");
            var privateKey = await _s3Client.UploadFileWithRandomKey(S3BucketKeys.PrivateProblems, packageZipPath);

            // Создать миссию в базе данных
            var dbMission = new DbMission
            {
                Author = existingUser,
                Name = form.Name,
                S3Key = privateKey,
                Difficulty = form.Difficulty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _missionRepository.CreateAsync(dbMission, cancellationToken);

            // Обработать теги
            await SyncMissionTagsAsync(dbMission, form.Tags, cancellationToken);

            // Обработать statements и медиа файлы
            await ProcessMissionStatementsAsync(dbMission, packageZipPath, cancellationToken);

            await _missionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Mission uploaded successfully: {MissionId}", dbMission.Id);
            var fullMission = await _missionRepository.FindWithDetailsAsync(dbMission.Id, cancellationToken);
            return MissionResponse.FromEntity(fullMission ?? dbMission, includeStatements: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading mission");
            return null;
        }
        finally
        {
            // Очистить временные файлы
            CleanupTemporaryFiles(unpackFolder, packageZipPath);
        }
    }

    public async Task<MissionsPageResponse?> GetMissionsListAsync(
        int pageSize,
        int pageNumber,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageSize <= 0 || pageNumber < 0)
            {
                _logger.LogWarning("Invalid pagination parameters: pageSize={PageSize}, pageNumber={PageNumber}", pageSize, pageNumber);
                return null;
            }

            IEnumerable<int>? tagIds = null;
            if (tags != null)
            {
                var normalized = tags
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (normalized.Count > 0)
                {
                    var existingTags = await _tagRepository.FindByNamesAsync(normalized, cancellationToken);
                    tagIds = existingTags.Select(t => t.Id).ToList();
                }
            }

            var (missions, hasNextPage) = await _missionRepository.GetFilteredPageAsync(pageSize, pageNumber, tagIds, cancellationToken);
            var apiList = missions.Select(m => MissionResponse.FromEntity(m, includeStatements: false));

            return new MissionsPageResponse(hasNextPage, apiList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting missions list");
            return null;
        }
    }

    public async Task<MissionResponse?> GetMissionAsync(int missionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var mission = await _missionRepository.FindWithDetailsAsync(missionId, cancellationToken);
            return mission == null ? null : MissionResponse.FromEntity(mission, includeStatements: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mission details: {MissionId}", missionId);
            return null;
        }
    }

    private async Task SyncMissionTagsAsync(DbMission mission, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        var normalized = tags?
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (normalized.Count == 0)
        {
            await _missionRepository.SyncTagsAsync(mission, Array.Empty<int>(), cancellationToken);
            return;
        }

        var existing = await _tagRepository.FindByNamesAsync(normalized, cancellationToken);
        var allTags = existing.ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);

        foreach (var tagName in normalized)
        {
            if (allTags.ContainsKey(tagName))
                continue;

            var newTag = new DbTag
            {
                Name = tagName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tagRepository.CreateAsync(newTag, cancellationToken);
            allTags[tagName] = newTag;
        }

        await _missionRepository.SyncTagsAsync(mission, allTags.Values.Select(t => t.Id), cancellationToken);
    }

    private async Task ProcessMissionStatementsAsync(DbMission mission, string zipFilePath, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing mission statements from archive");
            
            // Распаковать и обработать statements
            var statements = _archiveProcessor.ExtractStatements(zipFilePath);

            if (!statements.Any())
            {
                _logger.LogWarning("No statements found in archive for mission {MissionId}", mission.Id);
                return;
            }

            foreach (var (language, statementData) in statements)
            {
                var dbStatement = new DbMissionStatement
                {
                    Mission = mission,
                    Language = language,
                    StatementTexts = statementData.StatementTexts,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _missionRepository.CreateMissionStatementAsync(dbStatement, cancellationToken);

                // Загрузить медиа файлы (картинки)
                await UploadStatementMediaAsync(dbStatement, statementData.ImageFiles, zipFilePath, cancellationToken);
            }

            _logger.LogInformation("Successfully processed {StatementCount} statements for mission {MissionId}", 
                statements.Count, mission.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing mission statements");
        }
    }

    private async Task UploadStatementMediaAsync(
        DbMissionStatement statement,
        List<ImageFileData> imageFiles,
        string zipFilePath,
        CancellationToken cancellationToken)
    {
        if (!imageFiles.Any())
            return;

        using (var zipArchive = ZipFile.OpenRead(zipFilePath))
        {
            foreach (var imageData in imageFiles)
            {
                try
                {
                    var zipEntry = zipArchive.GetEntry(imageData.ZipEntry?.FullName ?? "");
                    if (zipEntry == null)
                    {
                        _logger.LogWarning("Image file not found in archive: {FileName}", imageData.FileName);
                        continue;
                    }

                    // Проверяем тип медиа
                    var mediaType = _mediaService.GetMediaType(imageData.FileName);
                    if (mediaType != MediaType.Images)
                    {
                        _logger.LogWarning("File is not an image: {FileName}", imageData.FileName);
                        continue;
                    }

                    // Сохранить временный файл
                    var tempImagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(imageData.FileName));
                    try
                    {
                        using (var zipStream = zipEntry.Open())
                        using (var fileStream = System.IO.File.Create(tempImagePath))
                        {
                            await zipStream.CopyToAsync(fileStream, cancellationToken);
                        }

                        // Загрузить на S3 через MediaService
                        using (var fileStream = new FileStream(tempImagePath, FileMode.Open, FileAccess.Read))
                        {
                            var form = new FormFile(fileStream, 0, fileStream.Length, "file", imageData.FileName);
                            var mediaUrl = await _mediaService.UploadMediaAsync(form, cancellationToken);

                            if (!string.IsNullOrEmpty(mediaUrl))
                            {
                                var media = new DbMissionStatementMedia
                                {
                                    Statement = statement,
                                    FileName = imageData.FileName,
                                    MediaKey = mediaUrl,
                                    MediaUrl = mediaUrl
                                };

                                await _missionRepository.CreateMissionStatementMediaAsync(media, cancellationToken);
                                _logger.LogInformation("Uploaded media for statement {StatementId}: {FileName}", 
                                    statement.Id, imageData.FileName);
                            }
                        }
                    }
                    finally
                    {
                        if (System.IO.File.Exists(tempImagePath))
                            System.IO.File.Delete(tempImagePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image file: {FileName}", imageData.FileName);
                }
            }
        }
    }

    private void CleanupTemporaryFiles(params string[] paths)
    {
        try
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                else if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up temporary files");
        }
    }
}
