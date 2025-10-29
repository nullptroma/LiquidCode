using LiquidCode.Domain.Enums;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.External.S3;
using LiquidCode.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services;

/// <summary>
/// Реализация бизнес-логики для медиа файлов
/// </summary>
public class MediaService : IMediaService
{
    private readonly IS3PublicBucketClient _s3Client;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IS3PublicBucketClient s3Client, ILogger<MediaService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<string> UploadMediaAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("File is empty or null");
            return string.Empty;
        }

        // Определить базовую папку на основе типа файла
        var baseFolder = GetMediaType(file.FileName);

        // Сохранить файл временно
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Загрузить в S3
            var key = await _s3Client.UploadFileWithRandomKey(baseFolder.ToString().ToLower(), tempPath);
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Failed to upload file to S3");
                return string.Empty;
            }

            // Получить URL вместо возврата key
            var url = await _s3Client.BuildFileUrl(key);
            _logger.LogInformation("File uploaded to S3 with key: {Key}, URL: {Url}", key, url);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media file");
            return string.Empty;
        }
        finally
        {
            // Удалить временный файл
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    public MediaType GetMediaType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        
        return extension switch
        {
            _ when MediaTypeExtensions.ImageExtensions.Contains(extension) => MediaType.Images,
            _ when MediaTypeExtensions.VideoExtensions.Contains(extension) => MediaType.Videos,
            _ when MediaTypeExtensions.AudioExtensions.Contains(extension) => MediaType.Audio,
            _ when MediaTypeExtensions.ArchiveExtensions.Contains(extension) => MediaType.Archives,
            _ when MediaTypeExtensions.DocumentExtensions.Contains(extension) => MediaType.Documents,
            _ => MediaType.Other
        };
    }
}