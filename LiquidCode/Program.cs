using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using LiquidCode;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Services.Authentication;
using LiquidCode.Domain.Services.Missions;
using LiquidCode.Domain.Services.Submits;
using LiquidCode.Infrastructure.Middleware;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Repositories;
using LiquidCode.Infrastructure.External.TestingModule;
using LiquidCode.Shared.Constants;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var dbConnectionString = new ConnectionStringParser(builder.Configuration[ConfigurationKeys.PostgresUri]!).EfCoreString;

if (builder.Configuration[ConfigurationKeys.DropDatabaseFlag] == "1")
{
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<LiquidDbContext>();
        optionsBuilder.UseNpgsql(dbConnectionString);
        var context = new LiquidDbContext(optionsBuilder.Options);
        var res = StartupMethods.DropDb(context);
        Console.WriteLine("Drop is complete!");
        return res ? 0 : 1;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

if (builder.Configuration[ConfigurationKeys.MigrateOnlyFlag] == "1")
{
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<LiquidDbContext>();
        optionsBuilder.UseNpgsql(dbConnectionString);
        var context = new LiquidDbContext(optionsBuilder.Options);
        var res = StartupMethods.Migrate(context);
        return res ? 0 : 1;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}


// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();

builder.Services.AddS3Buckets(builder.Configuration);
builder.Services.AddSingleton(new TestingHttpClient(builder.Configuration[ConfigurationKeys.TestingModuleUrl] ??
                                                    throw new ArgumentNullException(ConfigurationKeys.TestingModuleUrl)));

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<ISubmitRepository, SubmitRepository>();

// Add services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<ISubmitService, SubmitService>();

builder.Services.AddDbContext<LiquidDbContext>(options =>
    options.UseNpgsql(dbConnectionString).UseSnakeCaseNamingConvention());

// JWT for asp net core
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration[ConfigurationKeys.JwtIssuer],
            ValidAudience = builder.Configuration[ConfigurationKeys.JwtAudience],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration[ConfigurationKeys.JwtSigningKey] ?? "0"))
        };
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
                new OpenApiInfo()
                {
                    Title = "LiquidCode API - V1",
                    Version = "v1"
                }
            );

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Global exception handling middleware (must be first!)
//app.UseExceptionHandling();

app.UseCors(builder => builder.AllowAnyOrigin());

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
return 0;