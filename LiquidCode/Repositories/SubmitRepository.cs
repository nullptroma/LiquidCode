using LiquidCode.Db;
using LiquidCode.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Repositories;

/// <summary>
/// Repository implementation for user submission-related database operations
/// </summary>
public class SubmitRepository : Repository<DbUserSubmit>, ISubmitRepository
{
    public SubmitRepository(LiquidDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<DbUserSubmit>> GetSubmissionsByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(s => s.User.Id == userId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<DbUserSubmit>> GetSubmissionsByMissionAsync(int missionId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(s => s.Solution!.Mission.Id == missionId)
            .OrderByDescending(s => s.Solution!.Time)
            .ToListAsync(cancellationToken);

    public async Task<DbUserSubmit?> GetSubmissionWithDetailsAsync(int submissionId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(s => s.User)
            .Include(s => s.Solution)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

    public async Task<DbSolution?> GetSolutionAsync(int solutionId, CancellationToken cancellationToken = default) =>
        await DbContext.Solutions
            .FirstOrDefaultAsync(s => s.Id == solutionId, cancellationToken);

    public async Task AddSolutionAsync(DbSolution solution, CancellationToken cancellationToken = default) =>
        await DbContext.Solutions.AddAsync(solution, cancellationToken);
}
