namespace LiquidCode.Services.S3ClientService;

public interface IS3PublicBucketClient : IS3BucketClient
{
    string GetPublicDownloadUrl(string key);
}