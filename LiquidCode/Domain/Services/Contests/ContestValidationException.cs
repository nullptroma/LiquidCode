using System;

namespace LiquidCode.Domain.Services.Contests;

/// <summary>
/// Исключение, сигнализирующее о бизнес-ошибках при создании или обновлении контеста
/// </summary>
public class ContestValidationException : Exception
{
    public ContestValidationException(string message) : base(message)
    {
    }
}
