using LiquidCode.Services.S3ClientService;

namespace LiquidCode.Services;

public static class ServicesExtensions
{
    public static void AddS3Buckets(
        this IServiceCollection services, IConfiguration config)
    {
        var privateBucketName = config[ConfigurationStrings.S3PrivateBucket] ??
                                throw new ArgumentNullException(ConfigurationStrings.S3PrivateBucket);
        services.AddSingleton<IS3BucketClient, S3BucketClient>(provider =>
            new S3BucketClient(provider.GetRequiredService<IConfiguration>(), new Bucket(privateBucketName, false))
        );

        var publicBucketName = config[ConfigurationStrings.S3PublicBucket] ??
                               throw new ArgumentNullException(ConfigurationStrings.S3PublicBucket);
        services.AddSingleton<IS3PublicBucketClient, S3PublicBucketClient>(provider =>
            new S3PublicBucketClient(provider.GetRequiredService<IConfiguration>(), new Bucket(publicBucketName, true))
        );
    }
}