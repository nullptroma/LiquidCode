using System;
using System.Collections.Generic;
using System.Linq;
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
    private const decimal MissionSolvedScore = 1m;

    private readonly ISubmitRepository _submitRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IContestRepository _contestRepository;
    private readonly ILogger<SubmitService> _logger;
    private readonly IS3BucketClient _s3Client;
    private readonly TestingHttpClient _testingClient;
    private readonly ISubmitCallbackTokenService _callbackTokenService;

    private static readonly TimeSpan PackageLinkLifetime = TimeSpan.FromHours(1);

    public SubmitService(
        ISubmitRepository submitRepository,
        IMissionRepository missionRepository,
        IUserRepository userRepository,
        IContestRepository contestRepository,
        ILogger<SubmitService> logger,
        IS3BucketClient s3Client,
        TestingHttpClient testingClient,
        ISubmitCallbackTokenService callbackTokenService)
    {
        _submitRepository = submitRepository;
        _missionRepository = missionRepository;
        _userRepository = userRepository;
        _contestRepository = contestRepository;
        _logger = logger;
        _s3Client = s3Client;
        _testingClient = testingClient;
        _callbackTokenService = callbackTokenService;
    }

    public async Task<DbSolution?> SubmitSolutionAsync(
        int missionId,
        int userId,
        string sourceCode,
        string language,
        string languageVersion,
        int? contestAttemptId,
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
            DbContestAttempt? contestAttempt = null;
            DbContestMembership? contestMembership = null;

            var finalSourceType = SubmissionSourceType.Direct;
            if (contestAttemptId.HasValue)
            {
                contestAttempt = await _contestRepository.FindAttemptWithDetailsAsync(contestAttemptId.Value, cancellationToken);
                if (contestAttempt == null)
                {
                    _logger.LogWarning("Contest attempt not found: {AttemptId}", contestAttemptId);
                    return null;
                }

                if (contestAttempt.UserId != userId)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to submit for attempt {AttemptId} owned by {OwnerId}",
                        userId,
                        contestAttempt.Id,
                        contestAttempt.UserId);
                    return null;
                }

                contest = contestAttempt.Contest;
                if (contest == null || contest.IsDeleted)
                {
                    _logger.LogWarning("Contest for attempt {AttemptId} is not available", contestAttempt.Id);
                    return null;
                }

                contestMembership = contestAttempt.Membership;
                if (contestMembership == null)
                {
                    _logger.LogWarning("Contest membership missing for attempt {AttemptId}", contestAttempt.Id);
                    return null;
                }

                if (!contest.Missions.Any(cm => cm.MissionId == missionId))
                {
                    _logger.LogWarning("Mission {MissionId} is not part of contest {ContestId}", missionId, contest.Id);
                    return null;
                }

                var now = DateTime.UtcNow;
                if (contestAttempt.ExpiresAt.HasValue && now > contestAttempt.ExpiresAt.Value)
                {
                    _logger.LogInformation(
                        "Contest attempt expired before submission: AttemptId={AttemptId}, UserId={UserId}",
                        contestAttempt.Id,
                        userId);

                    await MarkAttemptExpiredAsync(contestMembership, contestAttempt, cancellationToken);
                    return null;
                }

                if (contestAttempt.Status != ContestAttemptStatus.Active)
                {
                    _logger.LogWarning(
                        "Contest attempt is not active: AttemptId={AttemptId}, Status={Status}",
                        contestAttempt.Id,
                        contestAttempt.Status);
                    return null;
                }

                var isOrganizer = contestMembership.Role.HasFlag(ContestMembershipRole.Organizer);
                switch (contest.ScheduleType)
                {
                    case ContestScheduleType.FixedWindow:
                        if (!contest.StartsAt.HasValue || !contest.EndsAt.HasValue)
                        {
                            _logger.LogWarning("Contest {ContestId} has inconsistent fixed window configuration", contest.Id);
                            return null;
                        }

                        if (!isOrganizer && (now < contest.StartsAt.Value || now > contest.EndsAt.Value))
                        {
                            _logger.LogWarning("Contest {ContestId} is not active for user {UserId}", contest.Id, userId);
                            return null;
                        }

                        finalSourceType = SubmissionSourceType.Contest;
                        break;

                    case ContestScheduleType.RollingWindow:
                        if (!contest.StartsAt.HasValue || !contest.EndsAt.HasValue || !contest.AttemptDurationMinutes.HasValue)
                        {
                            _logger.LogWarning("Contest {ContestId} has inconsistent rolling window configuration", contest.Id);
                            return null;
                        }

                        if (!isOrganizer && (now < contest.StartsAt.Value || now > contest.EndsAt.Value))
                        {
                            _logger.LogWarning("Contest {ContestId} is not available for user {UserId}", contest.Id, userId);
                            return null;
                        }

                        finalSourceType = SubmissionSourceType.ContestFlexibleWindow;
                        break;

                    case ContestScheduleType.AlwaysOpen:
                        if (!contest.AttemptDurationMinutes.HasValue)
                        {
                            _logger.LogWarning("Contest {ContestId} has inconsistent always-open configuration", contest.Id);
                            return null;
                        }

                        if (!isOrganizer)
                        {
                            if (contest.StartsAt.HasValue && now < contest.StartsAt.Value)
                            {
                                _logger.LogWarning("Contest {ContestId} has not started yet for user {UserId}", contest.Id, userId);
                                return null;
                            }

                            if (contest.EndsAt.HasValue && now > contest.EndsAt.Value)
                            {
                                _logger.LogWarning("Contest {ContestId} is already finished for user {UserId}", contest.Id, userId);
                                return null;
                            }
                        }

                        finalSourceType = SubmissionSourceType.ContestFlexibleWindow;
                        break;

                    default:
                        _logger.LogWarning(
                            "Contest {ContestId} has unsupported schedule type {ScheduleType}",
                            contest.Id,
                            contest.ScheduleType);
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
                Time = DateTime.UtcNow
            };

            // Создать отправку
            var submission = new DbUserSubmission
            {
                User = user,
                Solution = solution,
                Contest = contest,
                ContestId = contest?.Id,
                ContestAttemptId = contestAttempt?.Id,
                SourceType = finalSourceType
            };

            await _submitRepository.CreateAsync(submission, cancellationToken);

            if (contestAttempt != null)
            {
                await IncrementAttemptSubmissionStatsAsync(contestAttempt, mission.Id, cancellationToken);
            }
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

    public async Task<ContestSubmissionsResult> GetUserContestSubmissionsAsync(int userId, int contestId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contest = await _contestRepository.FindWithDetailsAsync(contestId, cancellationToken);
            if (contest == null || contest.IsDeleted)
            {
                return new ContestSubmissionsResult(ContestSubmissionQueryStatus.ContestNotFound, Enumerable.Empty<DbUserSubmission>());
            }

            if (!contest.Memberships.Any(m => m.UserId == userId))
            {
                return new ContestSubmissionsResult(ContestSubmissionQueryStatus.AccessDenied, Enumerable.Empty<DbUserSubmission>());
            }

            var submissions = await _submitRepository.GetSubmissionsByUserAndContestAsync(userId, contestId, cancellationToken);
            return new ContestSubmissionsResult(ContestSubmissionQueryStatus.Success, submissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions for user {UserId} in contest {ContestId}", userId, contestId);
            return new ContestSubmissionsResult(ContestSubmissionQueryStatus.Error, Enumerable.Empty<DbUserSubmission>());
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
        const int MaxTestingMessageLength = 10_000;
        try
        {
            var submission = await _submitRepository.GetSubmissionBySolutionIdAsync(solutionId, cancellationToken);
            if (submission?.Solution == null)
            {
                _logger.LogWarning("Solution not found: {SolutionId}", solutionId);
                return new TesterCallbackUpdateResult(TesterCallbackUpdateStatus.NotFound, null);
            }

            var solution = submission.Solution;

            if (!_callbackTokenService.ValidateToken(solution, callbackToken))
            {
                _logger.LogWarning("Callback token mismatch for solution {SolutionId}", solutionId);
                return new TesterCallbackUpdateResult(TesterCallbackUpdateStatus.TokenMismatch, null);
            }

            var normalizedAmount = Math.Max(amountOfTests, 0);
            var normalizedCurrent = Math.Clamp(currentTest, 0, normalizedAmount > 0 ? normalizedAmount : int.MaxValue);
            var trimmedMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
            if (trimmedMessage != null && trimmedMessage.Length > MaxTestingMessageLength)
            {
                trimmedMessage = trimmedMessage[..MaxTestingMessageLength];
            }

            solution.TestingState = state;
            solution.TestingErrorCode = errorCode;
            solution.TestingMessage = trimmedMessage;
            solution.CurrentTest = normalizedCurrent;
            solution.AmountOfTests = normalizedAmount;
            solution.Status = ComposeStatus(state, errorCode, trimmedMessage, normalizedCurrent, normalizedAmount);

            await _submitRepository.SaveChangesAsync(cancellationToken);

            await ApplyTesterUpdateToAttemptAsync(submission, state, errorCode, cancellationToken);

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

    private async Task MarkAttemptExpiredAsync(DbContestMembership membership, DbContestAttempt attempt, CancellationToken cancellationToken)
    {
        attempt.Status = ContestAttemptStatus.Expired;
        attempt.FinishedBy = ContestAttemptFinishReason.Timer;
        attempt.FinishedAt = attempt.ExpiresAt ?? DateTime.UtcNow;
        membership.ActiveAttemptId = null;
        membership.ActiveAttempt = null;
        membership.UpdatedAt = DateTime.UtcNow;

        await _contestRepository.UpdateAttemptAsync(attempt, cancellationToken);
    }

    private async Task IncrementAttemptSubmissionStatsAsync(DbContestAttempt attempt, int missionId, CancellationToken cancellationToken)
    {
        var missionResult = await ResolveMissionResultAsync(attempt, missionId, cancellationToken);
        if (missionResult == null)
            return;

        missionResult.SubmissionCount += 1;
        missionResult.LastSubmissionAt = DateTime.UtcNow;

        await _contestRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTesterUpdateToAttemptAsync(DbUserSubmission submission, TesterState state, TesterErrorCode errorCode, CancellationToken cancellationToken)
    {
        if (submission.ContestAttemptId == null || submission.Solution?.Mission == null)
            return;

        var attempt = submission.ContestAttempt ?? await _contestRepository.FindAttemptWithDetailsAsync(submission.ContestAttemptId.Value, cancellationToken);
        if (attempt == null)
            return;

        var missionResult = await ResolveMissionResultAsync(attempt, submission.Solution.Mission.Id, cancellationToken);
        if (missionResult == null)
            return;

        var now = DateTime.UtcNow;
        missionResult.LastSubmissionAt = now;

        if (state == TesterState.Done && errorCode == TesterErrorCode.None)
        {
            var isFirstSolve = !missionResult.SolvedAt.HasValue;
            missionResult.SolvedAt ??= now;
            missionResult.FirstAcceptedAt ??= now;
            missionResult.BestSubmissionId = submission.Id;
            if (missionResult.HighestScore < MissionSolvedScore)
            {
                missionResult.HighestScore = MissionSolvedScore;
            }

            if (isFirstSolve)
            {
                attempt.SolvedCount += 1;
                attempt.TotalScore += missionResult.HighestScore;
            }
        }

        await _contestRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<DbContestAttemptMissionResult?> ResolveMissionResultAsync(DbContestAttempt attempt, int missionId, CancellationToken cancellationToken)
    {
        var missionResult = attempt.MissionResults.FirstOrDefault(r => r.MissionId == missionId);
        if (missionResult != null)
            return missionResult;

        missionResult = await _contestRepository.GetMissionResultAsync(attempt.Id, missionId, cancellationToken);
        if (missionResult != null)
        {
            attempt.MissionResults.Add(missionResult);
        }

        return missionResult;
    }

    private static string ComposeStatus(
        TesterState state,
        TesterErrorCode errorCode,
        string? message,
        int currentTest,
        int amountOfTests)
    {
        const int MaxStatusLength = 256;
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

        var composed = string.IsNullOrWhiteSpace(message)
            ? baseStatus
            : $"{baseStatus}: {message}";

        return composed.Length <= MaxStatusLength
            ? composed
            : composed[..MaxStatusLength];
    }
}
