using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на создание группы
/// </summary>
/// <param name="Name">Название группы</param>
/// <param name="Description">Описание группы</param>
public record CreateGroupRequest(
    [Required] [StringLength(ValidationLengths.Group.NameMax, MinimumLength = ValidationLengths.Group.NameMin)] string Name,
    [StringLength(ValidationLengths.Group.DescriptionMax)] string? Description
);
