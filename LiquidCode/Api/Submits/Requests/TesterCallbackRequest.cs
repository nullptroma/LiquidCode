using System.Text.Json.Serialization;
using LiquidCode.Api.Submits.Dto;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Модель запроса обратного вызова от тестирующего модуля
/// </summary>
public sealed record TesterCallbackRequest(
    [property: JsonPropertyName("SubmitId")] long SubmitId,
    [property: JsonPropertyName("State")] TesterState State,
    [property: JsonPropertyName("ErrorCode")] TesterErrorCode ErrorCode,
    [property: JsonPropertyName("Message")] string? Message,
    [property: JsonPropertyName("CurrentTest")] int CurrentTest,
    [property: JsonPropertyName("AmountOfTests")] int AmountOfTests
);
