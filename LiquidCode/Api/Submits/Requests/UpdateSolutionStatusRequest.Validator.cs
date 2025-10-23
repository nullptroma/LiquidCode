using FluentValidation;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Валидатор для запросов обновления статуса решения
/// </summary>
public class UpdateSolutionStatusRequestValidator : AbstractValidator<UpdateSolutionStatusRequest>
{
    public UpdateSolutionStatusRequestValidator()
    {
        RuleFor(x => x.SubmissionId)
            .GreaterThan(0)
            .WithMessage("Submission ID must be greater than 0");

        RuleFor(x => x.VerdictCode)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Verdict code must be non-negative")
            .LessThanOrEqualTo(10)
            .WithMessage("Verdict code must be between 0 and 10");

        RuleFor(x => x.TestCase)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Test case number must be non-negative")
            .When(x => x.TestCase.HasValue);

        RuleFor(x => x.TimeUsed)
            .Length(1, 50)
            .WithMessage("Time used must be between 1 and 50 characters")
            .Matches(@"^\d+(\.\d+)?\s*m?s$")
            .WithMessage("Time used must be in format like '100ms', '1.5s', '500'")
            .When(x => !string.IsNullOrEmpty(x.TimeUsed));
    }
}
