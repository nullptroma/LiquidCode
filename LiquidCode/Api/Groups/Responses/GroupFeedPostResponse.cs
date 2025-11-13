using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Ответ о посте ленты группы
/// </summary>
/// <param name="Id">Идентификатор поста</param>
/// <param name="GroupId">Идентификатор группы</param>
/// <param name="AuthorId">Идентификатор автора</param>
/// <param name="AuthorUsername">Имя автора</param>
/// <param name="Name">Название поста</param>
/// <param name="Content">Содержимое поста</param>
/// <param name="CreatedAt">Дата и время создания</param>
/// <param name="UpdatedAt">Дата и время последнего обновления</param>
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
