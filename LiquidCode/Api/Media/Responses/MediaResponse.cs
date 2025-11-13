namespace LiquidCode.Api.Media.Responses;

/// <summary>
/// Ответ с информацией о загруженном медиа-файле
/// </summary>
public class MediaResponse
{
    /// <summary>
    /// URL загруженного медиа-файла
    /// </summary>
    public required string Url { get; set; }
}