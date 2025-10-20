namespace LiquidCode.Models.Constants;

/// <summary>
/// Application-wide constants for configuration, validation, and business logic
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Maximum number of refresh tokens allowed per user
    /// </summary>
    public const int MaxRefreshTokensPerUser = 50;

    /// <summary>
    /// JWT token expiration time in minutes
    /// </summary>
    public const int JwtExpirationMinutes = 2;

    /// <summary>
    /// Refresh token expiration time in days
    /// </summary>
    public const int RefreshTokenExpirationDays = 7;

    /// <summary>
    /// Maximum upload file size in MB
    /// </summary>
    public const int MaxUploadFileSizeMb = 100;

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public static readonly long MaxUploadFileSizeBytes = (long)MaxUploadFileSizeMb * 1024 * 1024;

    /// <summary>
    /// Valid programming languages for testing
    /// </summary>
    public static readonly string[] SupportedLanguages = { "cpp", "python", "java", "csharp" };

    /// <summary>
    /// Default programming language
    /// </summary>
    public const string DefaultLanguage = "cpp";

    /// <summary>
    /// Salt length for password hashing (bytes)
    /// </summary>
    public const int PasswordSaltLength = 32;

    /// <summary>
    /// Refresh token length (bytes)
    /// </summary>
    public const int RefreshTokenLength = 64;
}

/// <summary>
/// Environment variable configuration keys
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
    public const string S3PublicBucket = "S3_PUBLIC_BUCKET";
    public const string S3PrivateBucket = "S3_PRIVATE_BUCKET";
    public const string S3Endpoint = "S3_ENDPOINT";
    public const string TestingModuleUrl = "TESTING_MODULE_URL";
}

/// <summary>
/// S3 bucket configuration keys
/// </summary>
public static class S3BucketKeys
{
    public const string PrivateProblems = "problems";
    public const string PublicProblems = "problems-public";
}

/// <summary>
/// Mission statement file structure constants
/// </summary>
public static class MissionStatementPaths
{
    public const string StatementSectionsFolder = "statement-sections";
    public const string NameFile = "name.tex";
    public const string InputFile = "input.tex";
    public const string OutputFile = "output.tex";
    public const string LegendFile = "legend.tex";
    public const string ExampleFilePrefix = "example";
}
