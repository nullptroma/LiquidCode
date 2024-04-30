using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiquidCode.Models.Api.MissionsController;

public record UploadMissionForm(IFormFile MissionFile, string Name, [BindRequired] int Difficulty);