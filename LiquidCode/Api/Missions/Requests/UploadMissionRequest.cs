using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiquidCode.Api.Missions.Requests;

/// <summary>
/// Модель запроса для загрузки новой миссии
/// </summary>
/// <param name="MissionFile">Файл с миссией</param>
/// <param name="Name">Название миссии</param>
/// <param name="Difficulty">Уровень сложности миссии</param>
/// <param name="Tags">Теги миссии</param>
public record UploadMissionRequest(
    IFormFile MissionFile,
    string Name,
    [BindRequired] int Difficulty,
    IEnumerable<string>? Tags
);
