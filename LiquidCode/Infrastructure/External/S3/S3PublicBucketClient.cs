using System.Security.Policy;

namespace LiquidCode.Infrastructure.External.S3;

public class S3PublicBucketClient(IConfiguration conf, Bucket bucket) : S3BucketClient(conf, bucket), IS3PublicBucketClient
{
    public string GetPublicDownloadUrl(string key)
    {
        return Client.Config.ServiceURL + BucketInfo.Name + "/" + key;
    }
}