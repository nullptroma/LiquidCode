using System.Collections.Immutable;
using System.Text;
using LiquidCode;
using LiquidCode.Db;
using LiquidCode.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();
builder.Services.AddS3Buckets(builder.Configuration);

// Data base connection
var connectionString = new ConnectionStringParser(builder.Configuration[ConfigurationStrings.PgUri]!).EfCoreString;
builder.Services.AddDbContext<LiquidDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

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
            ValidIssuer = builder.Configuration[ConfigurationStrings.JwtIssuer],
            ValidAudience = builder.Configuration[ConfigurationStrings.JwtAudience],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration[ConfigurationStrings.JwtSigningKey] ?? "0"))
        };
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("LowCorsPolicy", corsBuilder =>
{
    corsBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();
var startup = new StartupMethods(app);

if (app.Configuration[ConfigurationStrings.MigrateOnly] == "1")
    try
    {
        var res = startup.Migrate(connectionString);
        return res ? 0 : 1;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

if (app.Configuration[ConfigurationStrings.DropDatabase] == "1")
    try
    {
        var res = startup.DropDb(connectionString);
        return res ? 0 : 1;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

app.UseCors("LowCorsPolicy");

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