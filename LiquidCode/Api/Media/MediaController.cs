using LiquidCode.Api.Media.Requests;
using LiquidCode.Api.Media.Responses;
using LiquidCode.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiquidCode.Api.Media;

/// <summary>
/// Контроллер для управления медиа файлами
/// </summary>
[Route("media")]
[ApiController]
public class MediaController(IMediaService mediaService) : ControllerBase
{
    /// <summary>
    /// Загружает медиа файл в публичное S3 хранилище
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadMediaRequest request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var key = await mediaService.UploadMediaAsync(request.File, cancellationToken);
        if (string.IsNullOrEmpty(key))
        {
            return StatusCode(500, "Failed to upload file.");
        }

        return await Download(key, cancellationToken);
    }

    /// <summary>
    /// Получает ссылку на скачивание медиа файла
    /// </summary>
    [HttpGet("download/{key}")]
    public async Task<IActionResult> Download([FromRoute] string key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Key is required.");
        }

        var url = await mediaService.GetMediaUrlAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(url))
        {
            return NotFound("File not found.");
        }

        return Ok(new MediaResponse { Url = url });
    }
}