using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Tags.Responses;

/// <summary>
/// Ответ API для тега
/// </summary>
public record TagResponse(
    int Id,
    string Name
)
{
    public static TagResponse FromEntity(DbTag entity) => new(entity.Id, entity.Name);
}
