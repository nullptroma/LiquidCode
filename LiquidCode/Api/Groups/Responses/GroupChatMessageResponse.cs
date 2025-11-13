using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Ответ на сообщение чата группы
/// </summary>
public record GroupChatMessageResponse(
    long Id,
    int GroupId,
    int AuthorId,
    string AuthorUsername,
    string Content,
    DateTime CreatedAt)
{
    public static GroupChatMessageResponse FromEntity(DbGroupChatMessage entity)
    {
        var username = entity.Author?.Username ?? string.Empty;
        return new GroupChatMessageResponse(
            entity.Id,
            entity.GroupId,
            entity.AuthorId,
            username,
            entity.Content,
            entity.CreatedAt);
    }
}
