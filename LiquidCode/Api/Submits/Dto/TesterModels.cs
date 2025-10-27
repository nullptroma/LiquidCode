using System.Text.Json.Serialization;

namespace LiquidCode.Api.Submits.Dto;

/// <summary>
/// Состояние выполнения решения в модуле тестирования
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TesterState
{
	Waiting,
	Compiling,
	Testing,
	Done
}

/// <summary>
/// Код ошибки, возвращаемый модулем тестирования
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TesterErrorCode
{
	None,
	CompileError,
	RuntimeError,
	MemoryError,
	TimeLimitError,
	IncorrectAnswer,
	UnknownError
}
