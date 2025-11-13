namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на обновление группы
/// </summary>
/// <param name="Name">Новое название группы</param>
/// <param name="Description">Новое описание группы</param>
public record UpdateGroupRequest(
    string? Name,
    string? Description
);
