using System.Configuration;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Infrastructure.External.S3;

public class S3PublicBucketClient : S3BucketClient, IS3PublicBucketClient
{
    public S3PublicBucketClient(IConfiguration conf, Bucket bucket)
    : base(conf, bucket)
    { }

    public Task<string> BuildFileUrl(string key)
    {
        var baseUrl = new Uri(Client.Config.ServiceURL);
        var url = new Uri(baseUrl, $"{BucketInfo.Name}/{key}");
        return Task.FromResult(url.AbsoluteUri);
    }
}