using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Infrastructure.External.S3;

public class S3BucketClient : IS3BucketClient
{
    public Bucket BucketInfo { get; }
    protected AmazonS3Client Client { get; }

    public S3BucketClient(IConfiguration conf, Bucket bucket)
    {
        AmazonS3Config config = new AmazonS3Config
        {
            ServiceURL = conf[ConfigurationKeys.S3Endpoint],
            UseHttp = true,
            ForcePathStyle = true,
        };

        AWSCredentials creds = new BasicAWSCredentials(conf[ConfigurationKeys.S3AccessKey], conf[ConfigurationKeys.S3SecretKey]);
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
        while (response.IsTruncated == true);

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

    public Task<string> GenerateDownloadLinkAsync(string key, TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Object key must be provided.", nameof(key));

        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be greater than zero.");

        var expiresAt = DateTime.UtcNow.Add(lifetime);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = BucketInfo.Name,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = expiresAt
        };

        var url = Client.GetPreSignedURL(request);
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException($"Failed to generate download link for key '{key}'.");

        return Task.FromResult(url);
    }
}