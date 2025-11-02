using System;

namespace LiquidCode.Infrastructure.External.S3;

public interface IS3BucketClient
{ 
    Bucket BucketInfo { get; }

    Task<List<string>> GetAllFiles();

    /// <summary>
    /// Загружает файл с случайным ключом
    /// </summary>
    /// <returns>Возвращает ключ на S3</returns>
    Task<string> UploadFileWithRandomKey(string baseFolder, string localFilePath);

    /// <summary>
    /// Генерирует временную ссылку для скачивания объекта.
    /// </summary>
    /// <param name="key">Ключ объекта в бакете.</param>
    /// <param name="lifetime">Срок действия ссылки.</param>
    /// <returns>Подписанная ссылка на объект.</returns>
    Task<string> GenerateDownloadLinkAsync(string key, TimeSpan lifetime);
}