using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LiquidCode.Api.Submits.Dto;
using LiquidCode.Domain.Interfaces.Repositories;
using LiquidCode.Domain.Interfaces.Services;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Infrastructure.External.S3;
using LiquidCode.Infrastructure.External.TestingModule;
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
    private readonly IS3BucketClient _s3Client;
    private readonly TestingHttpClient _testingClient;

    private static readonly TimeSpan PackageLinkLifetime = TimeSpan.FromHours(1);

    public SubmitService(
        ISubmitRepository submitRepository,
        IMissionRepository missionRepository,
        IUserRepository userRepository,
        IContestRepository contestRepository,
        ILogger<SubmitService> logger,
        IS3BucketClient s3Client,
        TestingHttpClient testingClient)
    {
        _submitRepository = submitRepository;
        _missionRepository = missionRepository;
        _userRepository = userRepository;
        _contestRepository = contestRepository;
        _logger = logger;
        _s3Client = s3Client;
        _testingClient = testingClient;
    }

    public async Task<DbSolution?> SubmitSolutionAsync(
        int missionId,
        int userId,
        string sourceCode,
        string language,
        string languageVersion,
        int? contestId,
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

            var finalSourceType = SubmissionSourceType.Direct;
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

                if (!contest.Missions.Any(cm => cm.MissionId == missionId))
                {
                    _logger.LogWarning("Mission {MissionId} is not part of contest {ContestId}", missionId, contestId);
                    return null;
                }

                var now = DateTime.UtcNow;
                switch (contest.ScheduleType)
                {
                    case ContestScheduleType.FixedWindow:
                        if (!contest.StartsAt.HasValue || !contest.EndsAt.HasValue)
                        {
                            _logger.LogWarning("Contest {ContestId} has inconsistent fixed window configuration", contestId);
                            return null;
                        }

                        if (!isOrganizer && (now < contest.StartsAt.Value || now > contest.EndsAt.Value))
                        {
                            _logger.LogWarning("Contest {ContestId} is not active for user {UserId}", contestId, userId);
                            return null;
                        }

                        if (finalSourceType == SubmissionSourceType.Direct)
                        {
                            finalSourceType = SubmissionSourceType.Contest;
                        }

                        break;

                    case ContestScheduleType.FlexibleWindow:
                        if (!contest.AvailableFrom.HasValue || !contest.AvailableUntil.HasValue || !contest.AttemptDurationMinutes.HasValue)
                        {
                            _logger.LogWarning("Contest {ContestId} has inconsistent flexible window configuration", contestId);
                            return null;
                        }

                        if (!isOrganizer)
                        {
                            if (now < contest.AvailableFrom.Value || now > contest.AvailableUntil.Value)
                            {
                                _logger.LogWarning("Contest {ContestId} is not available for user {UserId}", contestId, userId);
                                return null;
                            }

                            if (membership.ActiveAttemptStartedAt == null || membership.ActiveAttemptExpiresAt == null)
                            {
                                _logger.LogWarning("User {UserId} did not start an attempt in contest {ContestId}", userId, contestId);
                                return null;
                            }

                            if (now > membership.ActiveAttemptExpiresAt.Value)
                            {
                                _logger.LogWarning("Attempt for user {UserId} in contest {ContestId} has expired", userId, contestId);
                                return null;
                            }
                        }

                        if (finalSourceType == SubmissionSourceType.Direct)
                        {
                            finalSourceType = SubmissionSourceType.ContestFlexibleWindow;
                        }

                        break;

                    default:
                        _logger.LogWarning("Contest {ContestId} has unsupported schedule type {ScheduleType}", contestId, contest.ScheduleType);
                        return null;
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
                Status = ComposeStatus(TesterState.Waiting, TesterErrorCode.None, null, 0, 0),
                TestingState = TesterState.Waiting,
                TestingErrorCode = TesterErrorCode.None,
                TestingMessage = null,
                CurrentTest = 0,
                AmountOfTests = 0,
                CallbackToken = GenerateCallbackToken(),
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

    public async Task<SubmitDispatchResult> DispatchSolutionAsync(DbSolution solution, string callbackUrl, CancellationToken cancellationToken = default)
    {
        if (solution == null)
            throw new ArgumentNullException(nameof(solution));

        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            _logger.LogWarning("Callback URL is missing for solution {SolutionId}", solution.Id);
            return SubmitDispatchResult.Failed("Callback URL is not configured.");
        }

        var mission = solution.Mission;
        if (mission == null)
        {
            _logger.LogError("Solution {SolutionId} does not contain mission details", solution.Id);
            return SubmitDispatchResult.Failed("Mission data is not available for solution.");
        }

        if (string.IsNullOrWhiteSpace(mission.S3Key))
        {
            _logger.LogError("Mission {MissionId} has no S3 key for solution {SolutionId}", mission.Id, solution.Id);
            return SubmitDispatchResult.Failed("Mission package key is not configured.");
        }

        try
        {
            var packageUrl = await _s3Client.GenerateDownloadLinkAsync(mission.S3Key, PackageLinkLifetime);

            var payload = new SubmitForTesterModel(
                solution.Id,
                mission.Id,
                solution.Language,
                solution.LanguageVersion,
                solution.SourceCode,
                packageUrl,
                callbackUrl);

            await _testingClient.SubmitAsync(payload, cancellationToken);

            _logger.LogInformation("Solution {SolutionId} dispatched to testing module", solution.Id);
            return SubmitDispatchResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch solution {SolutionId} to testing module", solution.Id);
            return SubmitDispatchResult.Failed("Failed to dispatch solution to testing module.");
        }
    }

    public async Task<TesterCallbackUpdateResult> UpdateTesterStatusAsync(
        int solutionId,
        string callbackToken,
        TesterState state,
        TesterErrorCode errorCode,
        string? message,
        int currentTest,
        int amountOfTests,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var solution = await _submitRepository.GetSolutionAsync(solutionId, cancellationToken);
            if (solution == null)
            {
                _logger.LogWarning("Solution not found: {SolutionId}", solutionId);
                return new TesterCallbackUpdateResult(TesterCallbackUpdateStatus.NotFound, null);
            }

            if (string.IsNullOrWhiteSpace(solution.CallbackToken) || !IsTokenMatch(solution.CallbackToken, callbackToken))
            {
                _logger.LogWarning("Callback token mismatch for solution {SolutionId}", solutionId);
                return new TesterCallbackUpdateResult(TesterCallbackUpdateStatus.TokenMismatch, null);
            }

            var normalizedAmount = Math.Max(amountOfTests, 0);
            var normalizedCurrent = Math.Clamp(currentTest, 0, normalizedAmount > 0 ? normalizedAmount : int.MaxValue);
            var trimmedMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();

            solution.TestingState = state;
            solution.TestingErrorCode = errorCode;
            solution.TestingMessage = trimmedMessage;
            solution.CurrentTest = normalizedCurrent;
            solution.AmountOfTests = normalizedAmount;
            solution.Status = ComposeStatus(state, errorCode, trimmedMessage, normalizedCurrent, normalizedAmount);
            solution.CallbackToken = null;

            await _submitRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Solution tester status updated: SolutionId={SolutionId}, State={State}, ErrorCode={ErrorCode}, CurrentTest={CurrentTest}, TotalTests={TotalTests}",
                solutionId,
                state,
                errorCode,
                normalizedCurrent,
                normalizedAmount);
            return new TesterCallbackUpdateResult(TesterCallbackUpdateStatus.Success, solution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tester status: {SolutionId}", solutionId);
            return new TesterCallbackUpdateResult(TesterCallbackUpdateStatus.Error, null);
        }
    }

    private static string ComposeStatus(
        TesterState state,
        TesterErrorCode errorCode,
        string? message,
        int currentTest,
        int amountOfTests)
    {
        var baseStatus = state switch
        {
            TesterState.Waiting => "Waiting",
            TesterState.Compiling => "Compiling",
            TesterState.Testing => amountOfTests > 0
                ? $"Testing {Math.Clamp(currentTest, 0, amountOfTests)}/{amountOfTests}"
                : "Testing",
            TesterState.Done => errorCode switch
            {
                TesterErrorCode.None => "Accepted",
                TesterErrorCode.CompileError => "Compilation error",
                TesterErrorCode.RuntimeError => "Runtime error",
                TesterErrorCode.MemoryError => "Memory limit exceeded",
                TesterErrorCode.TimeLimitError => "Time limit exceeded",
                TesterErrorCode.IncorrectAnswer => "Wrong answer",
                _ => "Unknown error"
            },
            _ => "Unknown state"
        };

        return string.IsNullOrWhiteSpace(message)
            ? baseStatus
            : $"{baseStatus}: {message}";
    }

    private static string GenerateCallbackToken()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLowerInvariant();
    }

    private static bool IsTokenMatch(string storedToken, string providedToken)
    {
        if (string.IsNullOrWhiteSpace(storedToken) || string.IsNullOrWhiteSpace(providedToken))
            return false;

    var storedBytes = Encoding.UTF8.GetBytes(storedToken.Trim().ToLowerInvariant());
    var providedBytes = Encoding.UTF8.GetBytes(providedToken.Trim().ToLowerInvariant());

        if (storedBytes.Length != providedBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(storedBytes, providedBytes);
    }
}
