using System.Linq;
using FluentValidation;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Shared;

/// <summary>
/// Расширения для построения единообразных правил валидации.
/// </summary>
public static class ValidationExtensions
{
    private const string UsernamePattern = @"^[a-zA-Z0-9_\-\.]+$";

    /// <summary>
    /// Применить правило обязательной строки с ограничением по длине.
    /// </summary>
    public static IRuleBuilderOptions<T, string> RequiredText<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string fieldName,
        LengthRange lengthRange)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage($"{fieldName} is required")
            .Length(lengthRange.Min, lengthRange.Max)
            .WithMessage($"{fieldName} must be between {lengthRange.Min} and {lengthRange.Max} characters");
    }

    /// <summary>
    /// Правило для необязательной строки с ограничением только по максимальной длине.
    /// </summary>
    public static IRuleBuilderOptions<T, string> OptionalText<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string fieldName,
        int maxLength)
    {
        return ruleBuilder
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} must not exceed {maxLength} characters");
    }

    /// <summary>
    /// Аналог OptionalText для nullable-строк.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> OptionalTextWhenProvided<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string fieldName,
        int maxLength)
    {
        return ruleBuilder
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} must not exceed {maxLength} characters");
    }

    /// <summary>
    /// Необязательное поле со строгим диапазоном длины.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> OptionalTextWithinRange<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string fieldName,
        LengthRange lengthRange)
    {
        return ruleBuilder
            .Must(value => value == null || value.Length >= lengthRange.Min)
            .WithMessage($"{fieldName} must be at least {lengthRange.Min} characters")
            .MaximumLength(lengthRange.Max)
            .WithMessage($"{fieldName} must not exceed {lengthRange.Max} characters");
    }

    /// <summary>
    /// Правило для необязательной nullable-строки с обязательной непустотой при наличии значения.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> OptionalNonEmptyText<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string fieldName,
        int maxLength)
    {
        return ruleBuilder
            .Must(value => value == null || !string.IsNullOrWhiteSpace(value))
            .WithMessage($"{fieldName} must not be empty when provided")
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} must not exceed {maxLength} characters");
    }

    /// <summary>
    /// Валидация имени пользователя.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidUsername<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .RequiredText("Username", ValidationLengths.User.Username)
            .Matches(UsernamePattern)
            .WithMessage("Username can only contain letters, numbers, underscore, hyphen, or dot");
    }

    /// <summary>
    /// Валидация email адреса.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .RequiredText("Email", ValidationLengths.User.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
    }

    /// <summary>
    /// Базовое правило для ввода пароля (без проверки сложности).
    /// </summary>
    public static IRuleBuilderOptions<T, string> PasswordInput<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .RequiredText("Password", ValidationLengths.User.Password);
    }

    /// <summary>
    /// Правило сильного пароля с проверкой сложности.
    /// </summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .PasswordInput()
            .Must(HasUpperCase)
            .WithMessage("Password must contain at least one uppercase letter")
            .Must(HasLowerCase)
            .WithMessage("Password must contain at least one lowercase letter")
            .Must(HasDigit)
            .WithMessage("Password must contain at least one digit");
    }

    /// <summary>
    /// Проверка refresh token.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidRefreshToken<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .Length(ValidationLengths.RefreshToken.TokenMin, ValidationLengths.RefreshToken.TokenMax)
            .WithMessage($"Refresh token must be between {ValidationLengths.RefreshToken.TokenMin} and {ValidationLengths.RefreshToken.TokenMax} characters");
    }

    private static bool HasUpperCase(string value) => value.Any(char.IsUpper);

    private static bool HasLowerCase(string value) => value.Any(char.IsLower);

    private static bool HasDigit(string value) => value.Any(char.IsDigit);
}
