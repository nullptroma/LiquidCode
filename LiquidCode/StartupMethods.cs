using LiquidCode.Db;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode;

public static class StartupMethods
{
    public static bool Migrate(LiquidDbContext db)
    {
        db.Database.Migrate();
        Console.WriteLine("Migration is complete!");
        return true;
    }

    public static bool DropDb(LiquidDbContext db)
    {
        db.Database.EnsureDeleted();
        return true;
    }
}