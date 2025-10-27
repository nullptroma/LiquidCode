namespace LiquidCode.Shared.Constants;

/// <summary>
/// Глобальные константы для конфигурации, валидации и бизнес-логики
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Максимальное количество токенов обновления, разрешенное на пользователя
    /// </summary>
    public const int MaxRefreshTokensPerUser = 50;

    /// <summary>
    /// Время истечения JWT токена в минутах
    /// </summary>
    public const int JwtExpirationMinutes = 1440; // TODO: убавить, день для удобства

    /// <summary>
    /// Время истечения токена обновления в днях
    /// </summary>
    public const int RefreshTokenExpirationDays = 7;

    /// <summary>
    /// Максимальный размер загружаемого файла в МБ
    /// </summary>
    public const int MaxUploadFileSizeMb = 100;

    /// <summary>
    /// Максимальный размер файла в байтах
    /// </summary>
    public static readonly long MaxUploadFileSizeBytes = (long)MaxUploadFileSizeMb * 1024 * 1024;

    /// <summary>
    /// Длина токена обновления (байты)
    /// </summary>
    public const int RefreshTokenLength = 64;

    /// <summary>
    /// Фактор работы BCrypt (выше = безопаснее, но медленнее, рекомендуется: 11-12)
    /// </summary>
    public const int BcryptWorkFactor = 12;
}


/// <summary>
/// Ключи конфигурации S3 bucket
/// </summary>
public static class S3BucketKeys
{
    public const string PrivateProblems = "problems";
    public const string PublicContent = "content";
}

/// <summary>
/// Константы структуры файлов описания миссии
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
