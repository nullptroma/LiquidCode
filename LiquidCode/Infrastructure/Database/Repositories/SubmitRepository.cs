using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Infrastructure.Database;
using LiquidCode.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Repositories;

/// <summary>
/// Реализация репозитория для операций с базой данных, связанных с отправками пользователей
/// </summary>
public class SubmitRepository : ISubmitRepository
{
    private readonly LiquidDbContext _dbContext;
    private readonly DbCrud<DbUserSubmit> _submitRepository;

    public SubmitRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _submitRepository = new DbCrud<DbUserSubmit>(dbContext);
    }

    // IRepository<DbUserSubmit> implementation (delegated to _submitRepository)
    public Task<DbUserSubmit?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _submitRepository.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbUserSubmit> Items, bool HasNextPage)> GetPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default) =>
        _submitRepository.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbUserSubmit entity, CancellationToken cancellationToken = default) =>
        _submitRepository.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbUserSubmit entity, CancellationToken cancellationToken = default) =>
        _submitRepository.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbUserSubmit entity, CancellationToken cancellationToken = default) =>
        _submitRepository.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbUserSubmit entity, CancellationToken cancellationToken = default) =>
        _submitRepository.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _submitRepository.SaveChangesAsync(cancellationToken);

    // ISubmitRepository specific methods
    public async Task<IEnumerable<DbUserSubmit>> GetSubmissionsByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmit>()
            .Where(s => s.User.Id == userId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<DbUserSubmit>> GetSubmissionsByMissionAsync(int missionId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmit>()
            .Where(s => s.Solution!.Mission.Id == missionId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<DbUserSubmit?> GetSubmissionWithDetailsAsync(int submissionId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmit>()
            .Include(s => s.User)
            .Include(s => s.Solution)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

    public async Task<DbSolution?> GetSolutionAsync(int solutionId, CancellationToken cancellationToken = default) =>
        await _dbContext.Solutions
            .FirstOrDefaultAsync(s => s.Id == solutionId, cancellationToken);

    public async Task AddSolutionAsync(DbSolution solution, CancellationToken cancellationToken = default) =>
        await _dbContext.Solutions.AddAsync(solution, cancellationToken);
}
