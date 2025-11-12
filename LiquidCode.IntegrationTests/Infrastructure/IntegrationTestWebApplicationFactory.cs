using System.Collections.Generic;
using System.Linq;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiquidCode.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private Dictionary<string, string?>? _environmentVariables;

    public IntegrationTestWebApplicationFactory(Dictionary<string, string?>? env)
    {
        _environmentVariables = env;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(_environmentVariables);
        });

        return base.CreateHost(builder);
    }
}
