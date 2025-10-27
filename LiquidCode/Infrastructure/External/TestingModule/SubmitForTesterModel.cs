using System.Text.Json.Serialization;

namespace LiquidCode.Infrastructure.External.TestingModule;

/// <summary>
/// DTO, отправляемая во внешний тестирующий модуль
/// </summary>
public sealed record SubmitForTesterModel(
    [property: JsonPropertyName("Id")] long Id,
    [property: JsonPropertyName("MissionId")] long MissionId,
    [property: JsonPropertyName("Language")] string Language,
    [property: JsonPropertyName("LanguageVersion")] string LanguageVersion,
    [property: JsonPropertyName("SourceCode")] string SourceCode,
    [property: JsonPropertyName("PackageUrl")] string PackageUrl,
    [property: JsonPropertyName("CallbackUrl")] string CallbackUrl
);
