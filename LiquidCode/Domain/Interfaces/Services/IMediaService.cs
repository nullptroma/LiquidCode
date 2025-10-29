using LiquidCode.Domain.Enums;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис управления медиа файлами
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Загружает медиа файл в публичное S3 хранилище
    /// </summary>
    /// <param name="file">Файл для загрузки</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Ссылка на скачивание файла</returns>
    Task<string> UploadMediaAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Определяет тип медиа файла по его имени
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public MediaType GetMediaType(string fileName);
}