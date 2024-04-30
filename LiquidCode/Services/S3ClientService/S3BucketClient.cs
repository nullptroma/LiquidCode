using System.Configuration;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace LiquidCode.Services.S3ClientService;

public class S3BucketClient : IS3BucketClient
{
    public Bucket BucketInfo { get; }
    protected AmazonS3Client Client { get; }

    public S3BucketClient(IConfiguration? conf, Bucket bucket)
    {
        AmazonS3Config config = new AmazonS3Config
        {
            ServiceURL = conf[ConfigurationStrings.S3Endpoint],
            UseHttp = true,
            ForcePathStyle = true,
        };

        AWSCredentials creds = new BasicAWSCredentials(conf[ConfigurationStrings.S3Access], conf[ConfigurationStrings.S3Secret]);
        Client = new AmazonS3Client(creds, config);
        BucketInfo = bucket;
    }
    
    public async Task<List<string>> GetAllFiles()
    {
        var request = new ListObjectsV2Request
        {
            BucketName = BucketInfo.Name,
            MaxKeys = 15,
        };

        ListObjectsV2Response response;
        List<string> keys = [];
        do
        {
            response = await Client.ListObjectsV2Async(request);

            keys.AddRange(response.S3Objects.Select(obj=>obj.Key));

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated);

        return keys;
    }

    public async Task<string> UploadFileWithRandomKey(string baseFolder, string localFilePath)
    {
        if (!File.Exists(localFilePath))
            return "";
        var ext = Path.GetExtension(localFilePath);
        var name = Guid.NewGuid().ToString();
        var key = Path.Combine(baseFolder, name + ext);
        
        var request = new PutObjectRequest
        {
            BucketName = BucketInfo.Name,
            Key = key,
            FilePath = localFilePath,
        };

        var response = await Client.PutObjectAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK ? key : "";
    }
}