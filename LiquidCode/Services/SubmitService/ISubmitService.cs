using LiquidCode.Models.Database;

namespace LiquidCode.Services.SubmitService;

/// <summary>
/// Service interface for user submission-related operations
/// </summary>
public interface ISubmitService
{
    /// <summary>
    /// Submits a solution for a mission
    /// </summary>
    /// <param name="missionId">Mission ID</param>
    /// <param name="userId">User ID submitting the solution</param>
    /// <param name="sourceCode">Source code content</param>
    /// <param name="language">Programming language</param>
    /// <param name="languageVersion">Programming language version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created solution or null if submission failed</returns>
    Task<DbSolution?> SubmitSolutionAsync(
        int missionId, int userId, string sourceCode, string language, string languageVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific submission
    /// </summary>
    /// <param name="submissionId">Submission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Submission with related data or null if not found</returns>
    Task<DbUserSubmit?> GetSubmissionAsync(int submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all submissions by a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of submissions</returns>
    Task<IEnumerable<DbUserSubmit>> GetUserSubmissionsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all submissions for a mission
    /// </summary>
    /// <param name="missionId">Mission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of submissions</returns>
    Task<IEnumerable<DbUserSubmit>> GetMissionSubmissionsAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates solution status
    /// </summary>
    /// <param name="solutionId">Solution ID</param>
    /// <param name="status">New status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated solution or null if not found</returns>
    Task<DbSolution?> UpdateSolutionStatusAsync(int solutionId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a programming language is supported
    /// </summary>
    /// <param name="language">Language to validate</param>
    /// <returns>True if language is supported, false otherwise</returns>
    bool IsLanguageSupported(string language);
}
