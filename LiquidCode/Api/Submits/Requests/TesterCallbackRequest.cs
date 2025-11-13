using System.Text.Json.Serialization;
using LiquidCode.Api.Submits.Dto;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Модель запроса обратного вызова от тестирующего модуля
/// </summary>
/// <param name="SubmitId">Идентификатор отправленного решения</param>
/// <param name="State">Состояние выполнения решения</param>
/// <param name="ErrorCode">Код ошибки тестирования</param>
/// <param name="Message">Сообщение от тестирующего модуля</param>
/// <param name="CurrentTest">Номер текущего теста</param>
/// <param name="AmountOfTests">Общее количество тестов</param>
public sealed record TesterCallbackRequest(
    [property: JsonPropertyName("SubmitId")] long SubmitId,
    [property: JsonPropertyName("State")] TesterState State,
    [property: JsonPropertyName("ErrorCode")] TesterErrorCode ErrorCode,
    [property: JsonPropertyName("Message"), JsonConverter(typeof(CallbackMessageJsonConverter))] string? Message,
    [property: JsonPropertyName("CurrentTest")] int CurrentTest,
    [property: JsonPropertyName("AmountOfTests")] int AmountOfTests
);
