using System;
using System.Net.Http;
using System.Threading.Tasks;
using LiquidCode;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace LiquidCode.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgresContainer;
    private bool _disposed;
    private string? _connectionString;
    private string? _postgresUri;

    public IntegrationTestFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .WithDatabase("liquidcode_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _connectionString = _postgresContainer.GetConnectionString();
        var npgBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
        _postgresUri =
            $"postgresql://{npgBuilder.Username}:{npgBuilder.Password}@{npgBuilder.Host}:{npgBuilder.Port}/{npgBuilder.Database}";

        ApplyEnvironmentVariables();
        await RunMigrationsAsync();

        var configurationOverrides = new Dictionary<string, string?>
        {
        };

        Factory = new IntegrationTestWebApplicationFactory(_connectionString, configurationOverrides);

        // trigger application startup once so ASP.NET pipeline is ready for tests
        using var client = Factory.CreateClient();
    }

    public HttpClient CreateClient() => Factory.CreateClient();

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            Factory?.Dispose();
            await _postgresContainer.DisposeAsync();
            ClearEnvironmentVariables();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Factory?.Dispose();
            _postgresContainer.DisposeAsync().AsTask().GetAwaiter().GetResult();
            ClearEnvironmentVariables();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private void ApplyEnvironmentVariables()
    {
        if (string.IsNullOrWhiteSpace(_postgresUri))
            throw new InvalidOperationException("Postgres URI has not been initialised.");

        Environment.SetEnvironmentVariable(ConfigurationKeys.PostgresUri, _postgresUri);
        Environment.SetEnvironmentVariable(ConfigurationKeys.JwtIssuer, "test-issuer");
        Environment.SetEnvironmentVariable(ConfigurationKeys.JwtAudience, "test-audience");
        Environment.SetEnvironmentVariable(ConfigurationKeys.JwtSigningKey, "test-signing-key-1234567890");
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3AccessKey, "access-key");
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3SecretKey, "secret-key");
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3Endpoint, "http://localhost:9000");
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3PrivateBucket, "test-private");
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3PublicBucket, "test-public");
        Environment.SetEnvironmentVariable(ConfigurationKeys.ServiceBaseUrl, "http://localhost");
        Environment.SetEnvironmentVariable(ConfigurationKeys.TestingModuleUrl, "http://localhost/testing");
        Environment.SetEnvironmentVariable(ConfigurationKeys.SubmitCallbackSecret, "test-submit-secret");
    }

    private void ClearEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable(ConfigurationKeys.PostgresUri, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.JwtIssuer, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.JwtAudience, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.JwtSigningKey, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3AccessKey, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3SecretKey, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3Endpoint, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3PrivateBucket, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.S3PublicBucket, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.ServiceBaseUrl, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.TestingModuleUrl, null);
        Environment.SetEnvironmentVariable(ConfigurationKeys.SubmitCallbackSecret, null);
    }

    private Task RunMigrationsAsync()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Connection string has not been initialised.");

        var optionsBuilder = new DbContextOptionsBuilder<LiquidDbContext>();
        optionsBuilder.UseNpgsql(_connectionString).UseSnakeCaseNamingConvention();

        using var context = new LiquidDbContext(optionsBuilder.Options);
        StartupMethods.Migrate(context);
        return Task.CompletedTask;
    }
}
