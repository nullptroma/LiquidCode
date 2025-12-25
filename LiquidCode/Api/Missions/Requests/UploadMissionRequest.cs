using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiquidCode.Api.Missions.Requests;

/// <summary>
/// Модель запроса для загрузки новой миссии
/// </summary>
/// <param name="MissionFile">Файл с миссией</param>
/// <param name="Difficulty">Уровень сложности миссии</param>
/// <param name="Tags">
/// Теги миссии. Если null — берём из файла tags в архиве. Если пустой массив — теги не добавляем.
/// Если массив непустой — используем только его, игнорируя tags из архива.
/// </param>
public record UploadMissionRequest(
    IFormFile MissionFile,
    [BindRequired] int Difficulty,
    IEnumerable<string>? Tags
);
