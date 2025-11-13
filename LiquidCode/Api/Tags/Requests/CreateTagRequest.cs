using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Tags.Requests;

/// <summary>
/// Запрос на создание тега
/// </summary>
/// <param name="Name">Название тега</param>
public record CreateTagRequest(
    [Required] [StringLength(ValidationLengths.Tag.NameMax, MinimumLength = ValidationLengths.Tag.NameMin)] string Name
);
