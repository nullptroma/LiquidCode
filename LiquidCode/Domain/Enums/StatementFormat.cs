namespace LiquidCode.Domain.Enums;

/// <summary>
/// Формат текста миссии в архиве
/// </summary>
public enum StatementFormat
{
    /// <summary>
    /// Формат LaTeX (каталоги вида statements/&lt;language&gt;)
    /// </summary>
    Latex = 0,

    /// <summary>
    /// HTML представление (каталоги вида statements/.html/&lt;language&gt;)
    /// </summary>
    Html = 1
}
