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
    {
        AmazonS3Config config = new AmazonS3Config
        {
            ServiceURL = conf[ConfigurationKeys.S3Endpoint],
            UseHttp = true,
            ForcePathStyle = true,
        };

        AWSCredentials creds = new BasicAWSCredentials(conf[ConfigurationKeys.S3AccessKey], conf[ConfigurationKeys.S3SecretKey]);
    }
    
}