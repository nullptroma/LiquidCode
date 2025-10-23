using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiquidCode.Api.Missions.Requests;

/// <summary>
/// Модель запроса для загрузки новой миссии
/// </summary>
public record UploadMissionRequest(
    IFormFile MissionFile, 
    string Name, 
    [BindRequired] int Difficulty
);
