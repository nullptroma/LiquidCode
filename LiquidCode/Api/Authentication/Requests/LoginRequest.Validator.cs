using FluentValidation;
using LiquidCode.Api.Shared;

namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Валидатор для запросов входа
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// 
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .ValidUsername();

        RuleFor(x => x.Password)
            .PasswordInput();
    }
}
