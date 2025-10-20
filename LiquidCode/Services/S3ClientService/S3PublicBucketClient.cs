using System.Security.Policy;

namespace LiquidCode.Services.S3ClientService;

public class S3PublicBucketClient(IConfiguration conf, Bucket bucket) : S3BucketClient(conf, bucket), IS3PublicBucketClient
{
    public string GetPublicDownloadUrl(string key)
    {
        return Client.Config.ServiceURL + BucketInfo.Name + "/" + key;
    }
}