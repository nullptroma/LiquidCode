namespace LiquidCode.Shared.Options;

/// <summary>
/// Настройки генерации токена обратного вызова отправок.
/// </summary>
public sealed class SubmitCallbackTokenOptions
{
    public string? Secret { get; set; }
}
