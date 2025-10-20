using System.Text;
using LiquidCode;
using LiquidCode.Db;
using LiquidCode.Models.Constants;
using LiquidCode.Repositories;
using LiquidCode.Services;
using LiquidCode.Services.AuthService;
using LiquidCode.Services.MissionService;
using LiquidCode.Services.SubmitService;
using LiquidCode.Services.TestingModuleHttpClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

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