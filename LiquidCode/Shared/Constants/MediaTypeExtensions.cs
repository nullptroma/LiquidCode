namespace LiquidCode.Shared.Constants;

/// <summary>
/// Константы для определения типов медиа файлов
/// </summary>
public static class MediaTypeExtensions
{
    /// <summary>
    /// Расширения для изображений
    /// </summary>
    public static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".tiff",
        ".webp"
    };

    /// <summary>
    /// Расширения для видео
    /// </summary>
    public static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4",
        ".avi",
        ".mkv",
        ".mov",
        ".wmv",
        ".flv",
        ".webm"
    };

    /// <summary>
    /// Расширения для аудио
    /// </summary>
    public static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".flac",
        ".aac",
        ".ogg",
        ".wma"
    };

    /// <summary>
    /// Расширения для архивов
    /// </summary>
    public static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip",
        ".rar",
        ".7z",
        ".tar",
        ".gz",
        ".bz2"
    };

    /// <summary>
    /// Расширения для текстовых документов (включая JSON)
    /// </summary>
    public static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt",
        ".md",
        ".markdown",
        ".tex",
        ".latex",
        ".json"
    };
}
