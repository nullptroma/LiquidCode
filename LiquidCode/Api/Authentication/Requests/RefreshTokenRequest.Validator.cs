using FluentValidation;

namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Validator for refresh token requests
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
