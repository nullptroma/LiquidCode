using System.Linq;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace LiquidCode.Domain.Services.Submits;

/// <summary>
/// Реализация сервиса для операций, связанных с отправками пользователей
/// </summary>
public class SubmitService : ISubmitService
{
    private readonly ISubmitRepository _submitRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IContestRepository _contestRepository;
    private readonly ILogger<SubmitService> _logger;

    public SubmitService(
        ISubmitRepository submitRepository,
        IMissionRepository missionRepository,
        IUserRepository userRepository,
        IContestRepository contestRepository,
        ILogger<SubmitService> logger)
    {
        _submitRepository = submitRepository;
        _missionRepository = missionRepository;
        _userRepository = userRepository;
        _contestRepository = contestRepository;
        _logger = logger;
    }

    public async Task<DbSolution?> SubmitSolutionAsync(
        int missionId,
        int userId,
        string sourceCode,
        string language,
        string languageVersion,
        int? contestId,
        SubmissionSourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверить, существует ли миссия
            var mission = await _missionRepository.FindByIdAsync(missionId, cancellationToken);
            if (mission == null)
            {
                _logger.LogWarning("Mission not found: {MissionId}", missionId);
                return null;
            }

            // Проверить, существует ли пользователь
            var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            // Проверить, что исходный код не пуст
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                _logger.LogWarning("Source code is empty for user {UserId}", userId);
                return null;
            }

            DbContest? contest = null;
            var finalSourceType = sourceType;
            if (contestId.HasValue)
            {
                contest = await _contestRepository.FindWithDetailsAsync(contestId.Value, cancellationToken);
                if (contest == null)
                {
                    _logger.LogWarning("Contest not found: {ContestId}", contestId);
                    return null;
                }

                if (contest.IsDeleted)
                {
                    _logger.LogWarning("Contest is deleted: {ContestId}", contestId);
                    return null;
                }

                var membership = contest.Memberships.FirstOrDefault(m => m.UserId == userId);
                var isOrganizer = membership != null && membership.Role.HasFlag(ContestMembershipRole.Organizer);
                if (membership == null)
                {
                    _logger.LogWarning("User {UserId} is not enrolled in contest {ContestId}", userId, contestId);
                    return null;
                }

                var now = DateTime.UtcNow;
                if (!isOrganizer && (now < contest.StartsAt || now > contest.EndsAt))
                {
                    _logger.LogWarning("Contest {ContestId} is not active for user {UserId}", contestId, userId);
                    return null;
                }

                if (!contest.Missions.Any(cm => cm.MissionId == missionId))
                {
                    _logger.LogWarning("Mission {MissionId} is not part of contest {ContestId}", missionId, contestId);
                    return null;
                }

                if (finalSourceType == SubmissionSourceType.Direct)
                {
                    finalSourceType = SubmissionSourceType.ContestCompetition;
                }
            }

            if (contest == null)
            {
                finalSourceType = SubmissionSourceType.Direct;
            }

            // Создать решение
            var solution = new DbSolution
            {
                Mission = mission,
                Language = language,
                LanguageVersion = languageVersion,
                SourceCode = sourceCode,
                Status = "submitted",
                Time = DateTime.UtcNow
            };

            // Создать отправку
            var submission = new DbUserSubmission
            {
                User = user,
                Solution = solution,
                Contest = contest,
                ContestId = contest?.Id,
                SourceType = finalSourceType
            };

            await _submitRepository.CreateAsync(submission, cancellationToken);
            _logger.LogInformation("Solution submitted: UserId={UserId}, MissionId={MissionId}, SolutionId={SolutionId}", userId, missionId, solution.Id);

            return solution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting solution: UserId={UserId}, MissionId={MissionId}", userId, missionId);
            return null;
        }
    }

    public async Task<DbUserSubmission?> GetSubmissionAsync(int submissionId, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<DbUserSubmission>> GetUserSubmissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _submitRepository.GetSubmissionsByUserAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user submissions: {UserId}", userId);
            return Enumerable.Empty<DbUserSubmission>();
        }
    }

    public async Task<IEnumerable<DbUserSubmission>> GetMissionSubmissionsAsync(int missionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _submitRepository.GetSubmissionsByMissionAsync(missionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mission submissions: {MissionId}", missionId);
            return Enumerable.Empty<DbUserSubmission>();
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
            // TODO: Реализовать метод обновления в репозитории
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
