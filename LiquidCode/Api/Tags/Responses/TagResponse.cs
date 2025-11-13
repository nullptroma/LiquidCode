using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Tags.Responses;

/// <summary>
/// Ответ API для тега
/// </summary>
/// <param name="Id">Идентификатор тега</param>
/// <param name="Name">Название тега</param>
public record TagResponse(
    int Id,
    string Name
)
{
    public static TagResponse FromEntity(DbTag entity) => new(entity.Id, entity.Name);
}
