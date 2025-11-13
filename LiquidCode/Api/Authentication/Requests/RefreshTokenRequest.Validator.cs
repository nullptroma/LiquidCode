using FluentValidation;
using LiquidCode.Api.Shared;

namespace LiquidCode.Api.Authentication.Requests;

/// <summary>
/// Валидатор для запросов токенов обновления
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .ValidRefreshToken();
    }
}
