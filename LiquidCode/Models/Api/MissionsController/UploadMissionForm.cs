using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiquidCode.Models.Api.MissionsController;

// public record UploadMissionForm
// {
//     public IFormFile MissionFile { get; init; } = null!;
//     public string Name { get; init; } = "";
//     [BindRequired] public int Difficulty { get; init; }
// }


public record UploadMissionForm(IFormFile MissionFile, string Name, [BindRequired] int Difficulty);