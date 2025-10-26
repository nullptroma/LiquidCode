namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на обновление группы
/// </summary>
public record UpdateGroupRequest(
    string? Name,
    string? Description
);
