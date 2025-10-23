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
}