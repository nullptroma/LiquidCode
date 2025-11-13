using LiquidCode.Api.Groups.Requests;
using LiquidCode.Api.Groups.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис ленты группы
/// </summary>
public interface IGroupFeedService
{
    Task<GroupFeedPostResponse?> CreateAsync(int groupId, int authorId, CreateGroupFeedPostRequest request, CancellationToken cancellationToken = default);
    Task<GroupFeedPostResponse?> UpdateAsync(int groupId, int postId, int requesterId, UpdateGroupFeedPostRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int groupId, int postId, int requesterId, CancellationToken cancellationToken = default);
    Task<GroupFeedPostResponse?> GetAsync(int groupId, int postId, int requesterId, CancellationToken cancellationToken = default);
    Task<GroupFeedPageResponse?> GetPageAsync(int groupId, int requesterId, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
}
