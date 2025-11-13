using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Ответ на сообщение чата группы
/// </summary>
/// <param name="Id">Идентификатор сообщения</param>
/// <param name="GroupId">Идентификатор группы</param>
/// <param name="AuthorId">Идентификатор автора</param>
/// <param name="AuthorUsername">Имя автора</param>
/// <param name="Content">Текст сообщения</param>
/// <param name="CreatedAt">Дата и время создания</param>
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
