using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using LiquidCode;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Services.Authentication;
using LiquidCode.Domain.Services.Articles;
using LiquidCode.Domain.Services.Contests;
using LiquidCode.Domain.Services.Groups;
using LiquidCode.Domain.Services.Missions;
using LiquidCode.Domain.Services.Submits;
using LiquidCode.Domain.Services.Tags;
using LiquidCode.Domain.Services;
using LiquidCode.Infrastructure.Middleware;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Repositories;
using LiquidCode.Infrastructure.External.TestingModule;
using LiquidCode.Shared.Constants;
using LiquidCode.Shared.Options;
using LiquidCode.Shared.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Logging;
using LiquidCode.Domain.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var dbConnectionString = new ConnectionStringParser(builder.Configuration[ConfigurationKeys.PostgresUri]!).EfCoreString;

if (builder.Configuration[ConfigurationKeys.DropDatabaseFlag] == "1")
{
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<LiquidDbContext>();
        optionsBuilder.UseNpgsql(dbConnectionString).UseSnakeCaseNamingConvention();
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
        optionsBuilder.UseNpgsql(dbConnectionString).UseSnakeCaseNamingConvention();
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


// Добавить FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Настроить разрешающую политику CORS, чтобы браузеры могли отправлять запросы с
// пользовательскими заголовками (например, Content-Type) и предварительный OPTIONS запрос
// будет успешным. В продакшене следует ограничить origins/headers/methods.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddS3Buckets(builder.Configuration);
builder.Services.AddSingleton(provider =>
{
    var endpoint = builder.Configuration[ConfigurationKeys.TestingModuleUrl] ??
                    throw new ArgumentNullException(ConfigurationKeys.TestingModuleUrl);
    var logger = provider.GetRequiredService<ILogger<TestingHttpClient>>();
    return new TestingHttpClient(endpoint, logger);
});

builder.Services.AddOptions<SubmitCallbackTokenOptions>()
    .Configure(options =>
    {
        options.Secret = builder.Configuration[ConfigurationKeys.SubmitCallbackSecret] ??
            throw new InvalidOperationException($"Configuration value '{ConfigurationKeys.SubmitCallbackSecret}' is not provided.");
    });

builder.Services.AddSingleton<ISubmitCallbackTokenService, SubmitCallbackTokenService>();

// Добавить репозитории
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<ISubmitRepository, SubmitRepository>();
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IContestRepository, ContestRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();

// Добавить сервисы
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<ISubmitService, SubmitService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IContestService, ContestService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMediaService, MediaService>();

builder.Services.AddDbContext<LiquidDbContext>(options =>
    options.UseNpgsql(dbConnectionString).UseSnakeCaseNamingConvention());

// JWT для ASP.NET Core
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

// Добавить сервисы в контейнер.
// Подробнее о конфигурации Swagger/OpenAPI: https://aka.ms/aspnetcore/swashbuckle
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

// Глобальный middleware обработки исключений (должен быть первым!)
//app.UseExceptionHandling();

// Использовать именованную разрешающую политику, чтобы предварительные запросы включали
// Access-Control-Allow-Headers и другие необходимые заголовки.
app.UseCors("AllowAll");

// Настроить конвейер HTTP запросов.
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