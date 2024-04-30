namespace LiquidCode.Services.S3ClientService;

public interface IS3BucketClient
{ 
    Bucket BucketInfo { get; }

    Task<List<string>> GetAllFiles();

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns key on s3</returns>
    Task<string> UploadFileWithRandomKey(string baseFolder, string localFilePath);
}