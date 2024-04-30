using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Models.Database;

[Index(nameof(MissionId), nameof(Language))]
public class DbMissionPublicTextData
{
    public int Id { get; init; }
    [Required] public int? MissionId { get; init; }
    [StringLength(64)] public string Language { get; init; } = "";
    [StringLength(30000)] public string Data { get; init; } = "";
}