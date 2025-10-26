using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Api.Tags.Requests;

/// <summary>
/// Запрос на создание тега
/// </summary>
public record CreateTagRequest(
    [Required] [StringLength(64, MinimumLength = 2)] string Name
);
