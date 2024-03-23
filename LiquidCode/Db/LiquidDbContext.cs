using LiquidCode.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Db;

public class LiquidDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public LiquidDbContext(DbContextOptions<LiquidDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}