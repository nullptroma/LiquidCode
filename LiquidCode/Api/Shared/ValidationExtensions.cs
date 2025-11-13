using System;
using System.Collections.Generic;
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
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage($"{fieldName} must not be empty")
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
    /// Правило для положительных идентификаторов.
    /// </summary>
    public static IRuleBuilderOptions<T, int> PositiveId<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage($"{fieldName} must be greater than 0");
    }

    /// <summary>
    /// Правило для nullable идентификаторов.
    /// </summary>
    public static IRuleBuilderOptions<T, int?> OptionalPositiveId<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Must(id => !id.HasValue || id.Value > 0)
            .WithMessage($"{fieldName} must be greater than 0");
    }

    /// <summary>
    /// Валидация тегов.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidTagName<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        string fieldName = "Tag")
    {
        return ruleBuilder.RequiredText(fieldName, ValidationLengths.Tag.Name);
    }

    /// <summary>
    /// Правило для проверки списка тегов.
    /// </summary>
    public static IRuleBuilderOptionsConditions<T, IEnumerable<string>?> ValidTagsCollection<T>(
        this IRuleBuilder<T, IEnumerable<string>?> ruleBuilder,
        int maxCount,
        string fieldName = "Tags")
    {
        return ruleBuilder.Custom((tags, context) =>
        {
            if (tags == null)
            {
                return;
            }

            var tagList = tags.ToList();
            if (tagList.Count > maxCount)
            {
                context.AddFailure(fieldName, $"{fieldName} must not contain more than {maxCount} values");
            }

            var normalized = tagList
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag!.Trim().ToLowerInvariant())
                .ToList();

            if (normalized.Distinct().Count() != normalized.Count)
            {
                context.AddFailure(fieldName, $"{fieldName} must contain unique values");
            }
        });
    }

    /// <summary>
    /// Проверка enums с учетом флагов.
    /// </summary>
    public static IRuleBuilderOptions<T, TEnum> ValidEnumValue<T, TEnum>(
        this IRuleBuilder<T, TEnum> ruleBuilder,
        string fieldName,
        bool allowDefault = false)
        where TEnum : struct, Enum
    {
        var isFlagsEnum = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);

        var builder = allowDefault
            ? ruleBuilder
            : ruleBuilder
                .Must(value => !EqualityComparer<TEnum>.Default.Equals(value, default))
                .WithMessage($"{fieldName} is required");

        return builder
            .Must(value => IsValidEnumValue(value, isFlagsEnum))
            .WithMessage($"{fieldName} contains invalid value");
    }

    /// <summary>
    /// Nullable-перегрузка для enums.
    /// </summary>
    public static IRuleBuilderOptions<T, TEnum?> OptionalEnumValue<T, TEnum>(
        this IRuleBuilder<T, TEnum?> ruleBuilder,
        string fieldName,
        bool allowDefault = false)
        where TEnum : struct, Enum
    {
        var isFlagsEnum = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);

        var builder = ruleBuilder;

        if (!allowDefault)
        {
            builder = builder
                .Must(value => !value.HasValue || !EqualityComparer<TEnum>.Default.Equals(value.Value, default))
                .WithMessage($"{fieldName} is required");
        }

        return builder
            .Must(value => !value.HasValue || IsValidEnumValue(value.Value, isFlagsEnum))
            .WithMessage($"{fieldName} contains invalid value");
    }

    private static bool IsValidEnumValue<TEnum>(TEnum value, bool isFlagsEnum)
        where TEnum : struct, Enum
    {
        if (!isFlagsEnum)
        {
            return Enum.IsDefined(typeof(TEnum), value);
        }

        var numericValue = Convert.ToInt64(value);
        var allowedMask = AllowedFlagsCache<TEnum>.Mask;
        return (numericValue & ~allowedMask) == 0;
    }

    private static class AllowedFlagsCache<TEnum>
        where TEnum : struct, Enum
    {
        public static readonly long Mask = Enum
            .GetValues<TEnum>()
        .Select(value => Convert.ToInt64(value))
            .Aggregate(0L, (current, next) => current | next);
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
