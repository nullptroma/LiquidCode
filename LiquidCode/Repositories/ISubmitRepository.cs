using LiquidCode.Models.Database;

namespace LiquidCode.Repositories;

/// <summary>
/// Repository interface for user submission-related database operations
/// </summary>
public interface ISubmitRepository : IRepository<DbUserSubmit>
{
    /// <summary>
    /// Gets submissions by user
    /// </summary>
    Task<IEnumerable<DbUserSubmit>> GetSubmissionsByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets submissions by mission
    /// </summary>
    Task<IEnumerable<DbUserSubmit>> GetSubmissionsByMissionAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a submission with all related data
    /// </summary>
    Task<DbUserSubmit?> GetSubmissionWithDetailsAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets solution for a submission
    /// </summary>
    Task<DbSolution?> GetSolutionAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a solution for a submission
    /// </summary>
    Task AddSolutionAsync(DbSolution solution, CancellationToken cancellationToken = default);
}
