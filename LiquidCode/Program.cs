using System.Text;
using LiquidCode;
using LiquidCode.Db;
using LiquidCode.Services;
using LiquidCode.Services.TestingModuleHttpClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var dbConnectionString = new ConnectionStringParser(builder.Configuration[ConfigurationStrings.PgUri]!).EfCoreString;

if (builder.Configuration[ConfigurationStrings.DropDatabase] == "1")
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

if (builder.Configuration[ConfigurationStrings.MigrateOnly] == "1")
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
builder.Services.AddSingleton(new TestingHttpClient(builder.Configuration[ConfigurationStrings.TestingModuleUrl] ??
                                                    throw new ArgumentNullException(ConfigurationStrings
                                                        .TestingModuleUrl)));

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