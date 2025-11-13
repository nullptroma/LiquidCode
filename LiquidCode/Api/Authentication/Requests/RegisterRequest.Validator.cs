using FluentValidation;
using LiquidCode.Api.Shared;

namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Валидатор для запросов регистрации
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .ValidUsername();

        RuleFor(x => x.Email)
            .ValidEmail();

        RuleFor(x => x.Password)
            .StrongPassword();
    }
}
