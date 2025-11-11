using System.Collections.Generic;
using System.Linq;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiquidCode.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _efConnectionString;
    private readonly IReadOnlyDictionary<string, string?> _configurationOverrides;

    public IntegrationTestWebApplicationFactory(
        string efConnectionString,
        IReadOnlyDictionary<string, string?> configurationOverrides)
    {
        _efConnectionString = efConnectionString;
        _configurationOverrides = configurationOverrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(_configurationOverrides);
        });

        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<LiquidDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<LiquidDbContext>(options =>
                options.UseNpgsql(_efConnectionString).UseSnakeCaseNamingConvention());

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
            context.Database.Migrate();
        });
    }
}
