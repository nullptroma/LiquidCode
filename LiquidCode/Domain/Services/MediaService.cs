using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.External.S3;
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
        var baseFolder = GetBaseFolder(file.FileName);

        // Сохранить файл временно
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Загрузить в S3
            var key = await _s3Client.UploadFileWithRandomKey(baseFolder, tempPath);
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Failed to upload file to S3");
                return string.Empty;
            }

            _logger.LogInformation("File uploaded to S3 with key: {Key}", key);
            return key;
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

    public async Task<string> GetMediaUrlAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Key is empty or null");
            return string.Empty;
        }

        try
        {
            var url = await _s3Client.BuildFileUrl(key);
            _logger.LogInformation("Generated URL for key: {Key}", key);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating media URL for key: {Key}", key);
            return string.Empty;
        }
    }

    private static string GetBaseFolder(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".webp" => "images",
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".flv" or ".webm" => "videos",
            ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".wma" => "audio",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => "archives",
            _ => "other"
        };
    }
}