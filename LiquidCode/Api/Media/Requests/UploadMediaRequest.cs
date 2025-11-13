namespace LiquidCode.Api.Media.Requests;

/// <summary>
/// Запрос на загрузку медиа-файла
/// </summary>
public class UploadMediaRequest
{
    /// <summary>
    /// Загружаемый файл
    /// </summary>
    public required IFormFile File { get; set; }
}