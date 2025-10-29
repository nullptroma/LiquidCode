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
    /// <returns>Ключ файла в S3</returns>
    Task<string> UploadMediaAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает ссылку на скачивание медиа файла
    /// </summary>
    /// <param name="key">Ключ файла в S3</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Ссылка на файл</returns>
    Task<string> GetMediaUrlAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Определяет тип медиа файла по его имени
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public MediaType GetMediaType(string fileName);
}