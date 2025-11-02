namespace LiquidCode.Domain.Enums;

/// <summary>
/// Формат текста миссии в архиве
/// </summary>
public enum StatementFormat
{
    /// <summary>
    /// Формат LaTeX (каталоги вида statements/<language>)
    /// </summary>
    Latex = 0,

    /// <summary>
    /// HTML представление (каталоги вида statements/.html/<language>)
    /// </summary>
    Html = 1
}
