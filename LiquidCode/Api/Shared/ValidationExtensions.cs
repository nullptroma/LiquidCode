using FluentValidation;

namespace LiquidCode.Api.Shared;

/// <summary>
/// Extension методы для общих правил валидации
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Валидация имени пользователя
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidUsername<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Username is required")
            .Length(3, 128)
            .WithMessage("Username must be between 3 and 128 characters")
            .Matches(@"^[a-zA-Z0-9_\-\.]+$")
            .WithMessage("Username can only contain letters, numbers, underscore, hyphen, or dot");
    }
}
