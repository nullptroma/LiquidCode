using LiquidCode.Infrastructure.External.S3;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddS3Buckets(
        this IServiceCollection services, IConfiguration config)
    {
        var privateBucketName = config[ConfigurationKeys.S3PrivateBucket] ??
                                throw new ArgumentNullException(ConfigurationKeys.S3PrivateBucket);
        services.AddSingleton<IS3BucketClient, S3BucketClient>(provider =>
            new S3BucketClient(config, new Bucket(privateBucketName, false))
        );

        var publicBucketName = config[ConfigurationKeys.S3PublicBucket] ??
                                throw new ArgumentNullException(ConfigurationKeys.S3PublicBucket);
        services.AddSingleton<IS3PublicBucketClient, S3PublicBucketClient>(provider =>
            new S3PublicBucketClient(config, new Bucket(publicBucketName, true))
        );
    }
}