using System.Diagnostics;
using LiquidCode.Db;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode;

public class StartupMethods(WebApplication app)
{
    public async Task<bool> Migrate(string connectionString)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        Console.WriteLine($"Connection string: {connectionString}");
        db.Database.Migrate();
        Console.WriteLine("Migration is complete!");
        return true;
    }
    
    public async Task<bool> DropDb(string connectionString)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LiquidDbContext>();
        Console.WriteLine($"Connection string: {connectionString}");
        db.Database.EnsureDeleted();
        Console.WriteLine("Drop is complete!");
        return true;
    }
}