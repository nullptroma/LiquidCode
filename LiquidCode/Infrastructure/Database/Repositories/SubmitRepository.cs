using System.Collections.Generic;
using System.Linq;
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
    private readonly DbCrud<DbUserSubmission> _submitRepository;

    public SubmitRepository(LiquidDbContext dbContext)
    {
        _dbContext = dbContext;
        _submitRepository = new DbCrud<DbUserSubmission>(dbContext);
    }

    // IRepository<DbUserSubmit> implementation (delegated to _submitRepository)
    public Task<DbUserSubmission?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _submitRepository.FindByIdAsync(id, cancellationToken);

    public Task<(IEnumerable<DbUserSubmission> Items, bool HasNextPage)> GetPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default) =>
        _submitRepository.GetPageAsync(pageSize, pageNumber, cancellationToken);

    public Task CreateAsync(DbUserSubmission entity, CancellationToken cancellationToken = default) =>
        _submitRepository.CreateAsync(entity, cancellationToken);

    public Task UpdateAsync(DbUserSubmission entity, CancellationToken cancellationToken = default) =>
        _submitRepository.UpdateAsync(entity, cancellationToken);

    public Task DeleteAsync(DbUserSubmission entity, CancellationToken cancellationToken = default) =>
        _submitRepository.DeleteAsync(entity, cancellationToken);

    public Task SoftDeleteAsync(DbUserSubmission entity, CancellationToken cancellationToken = default) =>
        _submitRepository.SoftDeleteAsync(entity, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _submitRepository.SaveChangesAsync(cancellationToken);

    // ISubmitRepository specific methods
    public async Task<IEnumerable<DbUserSubmission>> GetSubmissionsByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmission>()
            .Include(s => s.User)
            .Include(s => s.Solution)
                .ThenInclude(sol => sol.Mission)
            .Include(s => s.Contest)
            .Where(s => s.User.Id == userId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<DbUserSubmission>> GetSubmissionsByMissionAsync(int missionId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmission>()
            .Include(s => s.User)
            .Include(s => s.Solution)
                .ThenInclude(sol => sol.Mission)
            .Include(s => s.Contest)
            .Where(s => s.Solution.Mission.Id == missionId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<DbUserSubmission>> GetSubmissionsByUserAndContestAsync(int userId, int contestId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmission>()
            .Include(s => s.User)
            .Include(s => s.Solution)
                .ThenInclude(sol => sol.Mission)
            .Include(s => s.Contest)
            .Where(s => s.User.Id == userId && s.ContestId == contestId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<DbUserSubmission?> GetSubmissionWithDetailsAsync(int submissionId, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<DbUserSubmission>()
            .Include(s => s.User)
            .Include(s => s.Solution)
                .ThenInclude(sol => sol.Mission)
            .Include(s => s.Contest)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

    public async Task<DbSolution?> GetSolutionAsync(int solutionId, CancellationToken cancellationToken = default) =>
        await _dbContext.Solutions
            .Include(s => s.Mission)
            .FirstOrDefaultAsync(s => s.Id == solutionId, cancellationToken);

    public async Task<DbUserSubmission?> GetSubmissionBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default) =>
        await _dbContext.UserSubmits
            .Include(s => s.Solution)
                .ThenInclude(sol => sol.Mission)
            .Include(s => s.ContestAttempt)
                .ThenInclude(a => a!.MissionResults)
            .FirstOrDefaultAsync(s => s.Solution.Id == solutionId, cancellationToken);

    public async Task AddSolutionAsync(DbSolution solution, CancellationToken cancellationToken = default) =>
        await _dbContext.Solutions.AddAsync(solution, cancellationToken);
}
