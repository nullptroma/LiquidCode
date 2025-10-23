using FluentValidation;

namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Валидатор для запросов токенов обновления
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(10)
            .WithMessage("Refresh token is invalid");
    }
}
