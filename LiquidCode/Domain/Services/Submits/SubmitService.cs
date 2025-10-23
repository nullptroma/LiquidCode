using LiquidCode.Shared.Constants;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Domain.Interfaces.Repositories;

namespace LiquidCode.Domain.Services.Submits;

/// <summary>
/// Service implementation for user submission-related operations
/// </summary>
public class SubmitService : ISubmitService
{
    private readonly ISubmitRepository _submitRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubmitService> _logger;

    public SubmitService(
        ISubmitRepository submitRepository,
        IMissionRepository missionRepository,
        IUserRepository userRepository,
        ILogger<SubmitService> logger)
    {
        _submitRepository = submitRepository;
        _missionRepository = missionRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<DbSolution?> SubmitSolutionAsync(
        int missionId, int userId, string sourceCode, string language, string languageVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate mission exists
            var mission = await _missionRepository.FindByIdAsync(missionId, cancellationToken);
            if (mission == null)
            {
                _logger.LogWarning("Mission not found: {MissionId}", missionId);
                return null;
            }

            // Validate user exists
            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            // Validate source code is not empty
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                _logger.LogWarning("Source code is empty for user {UserId}", userId);
                return null;
            }

            // Create solution
            var solution = new DbSolution
            {
                Mission = mission,
                Language = language,
                LanguageVersion = languageVersion,
                SourceCode = sourceCode,
                Status = "submitted",
                Time = DateTime.UtcNow
            };

            // Create submission
            var submission = new DbUserSubmit
            {
                User = user,
                Solution = solution
            };

            await _submitRepository.AddAsync(submission, cancellationToken);
            _logger.LogInformation("Solution submitted: UserId={UserId}, MissionId={MissionId}, SolutionId={SolutionId}", userId, missionId, solution.Id);

            return solution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting solution: UserId={UserId}, MissionId={MissionId}", userId, missionId);
            return null;
        }
    }

    public async Task<DbUserSubmit?> GetSubmissionAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _submitRepository.GetSubmissionWithDetailsAsync(submissionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission: {SubmissionId}", submissionId);
            return null;
        }
    }

    public async Task<IEnumerable<DbUserSubmit>> GetUserSubmissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _submitRepository.GetSubmissionsByUserAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user submissions: {UserId}", userId);
            return Enumerable.Empty<DbUserSubmit>();
        }
    }

    public async Task<IEnumerable<DbUserSubmit>> GetMissionSubmissionsAsync(int missionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _submitRepository.GetSubmissionsByMissionAsync(missionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mission submissions: {MissionId}", missionId);
            return Enumerable.Empty<DbUserSubmit>();
        }
    }

    public async Task<DbSolution?> UpdateSolutionStatusAsync(int solutionId, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var solution = await _submitRepository.GetSolutionAsync(solutionId, cancellationToken);
            if (solution == null)
            {
                _logger.LogWarning("Solution not found: {SolutionId}", solutionId);
                return null;
            }

            solution.Status = status;
            // TODO: Implement update method in repository
            await _submitRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Solution status updated: SolutionId={SolutionId}, Status={Status}", solutionId, status);
            return solution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating solution status: {SolutionId}", solutionId);
            return null;
        }
    }
}
