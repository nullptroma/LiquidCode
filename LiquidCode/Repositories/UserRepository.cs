using LiquidCode.Db;
using LiquidCode.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Repositories;

/// <summary>
/// Repository implementation for user-related database operations
/// </summary>
public class UserRepository : Repository<DbUser>, IUserRepository
{
    public UserRepository(LiquidDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<DbUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(u => u.Username == username, cancellationToken);

    public async Task<int> GetRefreshTokenCountAsync(int userId, CancellationToken cancellationToken = default) =>
        await DbContext.RefreshTokens.CountAsync(t => t.DbUser.Id == userId, cancellationToken);

    public async Task<DbRefreshToken?> GetOldestRefreshTokenAsync(int userId, CancellationToken cancellationToken = default) =>
        await DbContext.RefreshTokens
            .Where(t => t.DbUser.Id == userId)
            .OrderBy(t => t.Expires)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddRefreshTokenAsync(DbRefreshToken token, CancellationToken cancellationToken = default) =>
        await DbContext.RefreshTokens.AddAsync(token, cancellationToken);

    public async Task RemoveRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default)
    {
        var token = await DbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenString, cancellationToken);
        if (token != null)
            DbContext.RefreshTokens.Remove(token);
    }

    public async Task<DbRefreshToken?> FindRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default) =>
        await DbContext.RefreshTokens
            .Include(t => t.DbUser)
            .FirstOrDefaultAsync(t => t.Token == tokenString, cancellationToken);
}
