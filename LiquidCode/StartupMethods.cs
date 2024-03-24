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
        Stopwatch sw = Stopwatch.StartNew();
        while (db.Database.CanConnect() == false)
        {
            if (sw.ElapsedMilliseconds > 60000)
            {
                Console.WriteLine("Unable to connect to the database");
                return false;
            }
            await Task.Delay(200);
        }
        sw.Stop();
        Console.WriteLine($"Connected to db in {sw.ElapsedMilliseconds} ms");
        db.Database.Migrate();
        Console.WriteLine("Migration is complete!");
        return true;
    }
}