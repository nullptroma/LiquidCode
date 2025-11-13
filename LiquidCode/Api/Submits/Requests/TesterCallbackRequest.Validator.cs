using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Валидатор для обратных вызовов от тестирующего модуля
/// </summary>
public sealed class TesterCallbackRequestValidator : AbstractValidator<TesterCallbackRequest>
{
    public TesterCallbackRequestValidator()
    {
        RuleFor(x => x.SubmitId)
            .GreaterThan(0)
            .WithMessage("Submit ID must be greater than 0");

        RuleFor(x => x.AmountOfTests)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount of tests must be non-negative");

        RuleFor(x => x.CurrentTest)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Current test index must be non-negative")
            .LessThanOrEqualTo(x => x.AmountOfTests)
            .WithMessage("Current test cannot exceed total amount of tests")
            .When(x => x.AmountOfTests > 0);

        RuleFor(x => x.Message)
            .OptionalTextWhenProvided("Message", ValidationLengths.Solution.TestingMessageMax);
    }
}
