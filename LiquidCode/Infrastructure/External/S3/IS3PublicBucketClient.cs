namespace LiquidCode.Infrastructure.External.S3;

public interface IS3PublicBucketClient : IS3BucketClient
{ 
    /// <summary>
    /// Строит ссылку на объект в S3 для публичного скачивания.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<string> BuildFileUrl(string key);
}