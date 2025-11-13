using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Ответ о посте ленты группы
/// </summary>
public record GroupFeedPostResponse(
    int Id,
    int GroupId,
    int AuthorId,
    string AuthorUsername,
    string Name,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static GroupFeedPostResponse FromEntity(DbGroupFeedPost entity)
    {
        var username = entity.Author?.Username ?? string.Empty;
        return new GroupFeedPostResponse(
            entity.Id,
            entity.GroupId,
            entity.AuthorId,
            username,
            entity.Name,
            entity.Content,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
