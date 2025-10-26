using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на создание группы
/// </summary>
public record CreateGroupRequest(
    [Required] [StringLength(128, MinimumLength = 3)] string Name,
    string? Description
);
