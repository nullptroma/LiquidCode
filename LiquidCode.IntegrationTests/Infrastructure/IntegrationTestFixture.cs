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
    private Dictionary<string, string?>? _environmentVariables;

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

        Factory = new IntegrationTestWebApplicationFactory(_environmentVariables);

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

        _environmentVariables = new Dictionary<string, string?>
        {
            [ConfigurationKeys.PostgresUri] = _postgresUri,
            [ConfigurationKeys.JwtIssuer] = "test-issuer",
            [ConfigurationKeys.JwtAudience] = "test-audience",
            [ConfigurationKeys.JwtSigningKey] = "test-signing-key-1234567890vetyivfy4yfv34yvfy2f3f4",
            [ConfigurationKeys.S3AccessKey] = "access-key",
            [ConfigurationKeys.S3SecretKey] = "secret-key",
            [ConfigurationKeys.S3Endpoint] = "http://localhost:9000",
            [ConfigurationKeys.S3PrivateBucket] = "test-private",
            [ConfigurationKeys.S3PublicBucket] = "test-public",
            [ConfigurationKeys.ServiceBaseUrl] = "http://localhost",
            [ConfigurationKeys.TestingModuleUrl] = "http://localhost/testing",
            [ConfigurationKeys.SubmitCallbackSecret] = "test-submit-secret"
        };

        foreach (var kvp in _environmentVariables)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
    }

    private void ClearEnvironmentVariables()
    {
        if (_environmentVariables != null)
        {
            foreach (var key in _environmentVariables.Keys)
            {
                Environment.SetEnvironmentVariable(key, null);
            }
        }
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
