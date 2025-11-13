using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на создание группы
/// </summary>
/// <param name="Name">Название группы</param>
/// <param name="Description">Описание группы</param>
public record CreateGroupRequest(
    [Required] [StringLength(128, MinimumLength = 3)] string Name,
    string? Description
);
