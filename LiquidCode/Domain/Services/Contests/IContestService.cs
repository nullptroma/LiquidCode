using LiquidCode.Api.Contests.Requests;
using LiquidCode.Api.Contests.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Services.Contests;

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
    Task<bool> UpsertMemberAsync(int contestId, int requesterId, int targetUserId, ContestMembershipRole role, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(int contestId, int requesterId, int targetUserId, CancellationToken cancellationToken = default);
}
