using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiquidCode.Api.Missions.Requests;

/// <summary>
/// Request model for uploading a new mission
/// </summary>
public record UploadMissionRequest(
    IFormFile MissionFile, 
    string Name, 
    [BindRequired] int Difficulty
);
