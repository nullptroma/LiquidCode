using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Репозиторий для постов ленты группы
/// </summary>
public interface IGroupFeedRepository : IRepository<DbGroupFeedPost>
{
    Task<DbGroupFeedPost?> FindWithAuthorAsync(int groupId, int postId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<DbGroupFeedPost> Items, bool HasNextPage)> GetPageAsync(
        int groupId,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);
}
