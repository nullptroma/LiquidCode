
namespace LiquidCode.Shared.Constants;


/// <summary>
/// Ключи конфигурации переменных окружения
/// </summary>
public static class ConfigurationKeys
{
    public const string JwtIssuer = "JWT_ISSUER";
    public const string JwtAudience = "JWT_AUDIENCE";
    public const string JwtSigningKey = "JWT_SINGING_KEY";
    public const string PostgresUri = "PG_URI";
    public const string MigrateOnlyFlag = "MIGRATE_ONLY";
    public const string DropDatabaseFlag = "DROP_DATABASE";
    public const string S3AccessKey = "S3_ACCESS_KEY";
    public const string S3SecretKey = "S3_SECRET_KEY";
    public const string S3PrivateBucket = "S3_PRIVATE_BUCKET";
    public const string S3PublicBucket = "S3_PUBLIC_BUCKET";
    public const string S3Endpoint = "S3_ENDPOINT";
    public const string ServiceBaseUrl = "SERVICE_BASE_URL";
    public const string TestingModuleUrl = "TESTING_MODULE_URL";
}