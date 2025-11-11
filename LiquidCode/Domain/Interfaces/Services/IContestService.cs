using System.Collections.Generic;
using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Contests.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис управления контестами
/// </summary>
public interface IContestService
{
    Task<ContestResponse?> CreateAsync(CreateContestRequest request, int creatorId, CancellationToken cancellationToken = default);
    Task<ContestResponse?> UpdateAsync(int contestId, UpdateContestRequest request, int requesterId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int contestId, int requesterId, CancellationToken cancellationToken = default);
    Task<ContestResponse?> GetAsync(int contestId, CancellationToken cancellationToken = default);
    Task<ContestsPageResponse?> GetUpcomingAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<ContestsPageResponse?> GetForGroupAsync(int groupId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContestResponse>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<ContestsPageResponse?> GetParticipatingAsync(int userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<ContestMembersResult> GetMembersPageAsync(int contestId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
    Task<ContestAttemptsResult> GetUserAttemptsAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task<bool> UpsertMemberAsync(int contestId, int requesterId, int targetUserId, ContestMembershipRole role, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(int contestId, int requesterId, int targetUserId, CancellationToken cancellationToken = default);
    Task<ContestAttemptResponse?> StartAttemptAsync(int contestId, int userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserRegisteredAsync(int contestId, int userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Статус запроса участников контеста
/// </summary>
public enum ContestMembersQueryStatus
{
    Success,
    ContestNotFound,
    InvalidPagination,
    Error
}

/// <summary>
/// Результат запроса участников контеста
/// </summary>
public readonly record struct ContestMembersResult(
    ContestMembersQueryStatus Status,
    ContestMembersPageResponse? Page
);

/// <summary>
/// Статус запроса попыток пользователя в контесте
/// </summary>
public enum ContestAttemptQueryStatus
{
    Success,
    ContestNotFound,
    AccessDenied,
    Error
}

/// <summary>
/// Результат запроса попыток пользователя в контесте
/// </summary>
public readonly record struct ContestAttemptsResult(
    ContestAttemptQueryStatus Status,
    IReadOnlyList<ContestAttemptDetailsResponse> Attempts
);
