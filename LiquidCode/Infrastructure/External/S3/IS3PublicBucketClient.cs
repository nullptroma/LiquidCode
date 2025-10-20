namespace LiquidCode.Infrastructure.External.S3;

public interface IS3PublicBucketClient : IS3BucketClient
{
    string GetPublicDownloadUrl(string key);
}