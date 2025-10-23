using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Реализация репозитория для операций с базой данных, связанных с пользователями
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbUser> _userRepository;

    public UserRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _userRepository = new DbCrud<DbUser>(dbContext);
    }

    // IRepository<DbUser> implementation (delegated to _userRepository)
    public Task<DbUser?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _userRepository.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbUser> Items, bool HasNextPage)> GetPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default) =>
        _userRepository.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbUser entity, CancellationToken cancellationToken = default) =>
        _userRepository.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbUser entity, CancellationToken cancellationToken = default) =>
        _userRepository.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbUser entity, CancellationToken cancellationToken = default) =>
        _userRepository.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbUser entity, CancellationToken cancellationToken = default) =>
        _userRepository.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _userRepository.SaveChangesAsync(cancellationToken);

    // IUserRepository specific methods
    public async Task<DbUser?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default) =>
        await _dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public async Task<int> GetRefreshTokenCountAsync(int userId, CancellationToken cancellationToken = default) =>
        await _dbContext.RefreshTokens.CountAsync(t => t.DbUser.Id == userId, cancellationToken);

    public async Task<DbRefreshToken?> GetOldestRefreshTokenAsync(int userId, CancellationToken cancellationToken = default) =>
        await _dbContext.RefreshTokens
            .Where(t => t.DbUser.Id == userId)
            .OrderBy(t => t.Expires)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddRefreshTokenAsync(DbRefreshToken token, CancellationToken cancellationToken = default) =>
        await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);

    public async Task RemoveRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenString, cancellationToken);
        if (token != null)
            _dbContext.RefreshTokens.Remove(token);
    }

    public async Task<DbRefreshToken?> FindRefreshTokenAsync(string tokenString, CancellationToken cancellationToken = default) =>
        await _dbContext.RefreshTokens
            .Include(t => t.DbUser)
            .FirstOrDefaultAsync(t => t.Token == tokenString, cancellationToken);
}
