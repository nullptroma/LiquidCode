namespace LiquidCode;

public static class ConfigurationStrings
{
    public const string JwtIssuer = "JWT_ISSUER";
    public const string JwtAudience = "JWT_AUDIENCE";
    public const string JwtSigningKey = "JWT_SINGING_KEY";
    public const string PgUri = "PG_URI";
    public const string MigrateOnly = "MIGRATE_ONLY";
    public const string DropDatabase = "DROP_DATABASE";
    public const string S3Access = "S3_ACCESS_KEY";
    public const string S3Secret = "S3_SECRET_KEY";
    public const string S3PublicBucket = "S3_PUBLIC_BUCKET";
    public const string S3PrivateBucket = "S3_PRIVATE_BUCKET";
    public const string S3Endpoint = "S3_ENDPOINT";
    public const string TestingModuleUrl = "TESTING_MODULE_URL";
}