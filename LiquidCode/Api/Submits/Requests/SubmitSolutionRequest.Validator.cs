using FluentValidation;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Валидатор для запросов отправки решений
/// </summary>
public class SubmitSolutionRequestValidator : AbstractValidator<SubmitSolutionRequest>
{
    public SubmitSolutionRequestValidator()
    {
        RuleFor(x => x.MissionId)
            .GreaterThan(0)
            .WithMessage("Mission ID must be greater than 0");

        RuleFor(x => x.Language)
            .NotEmpty()
            .WithMessage("Programming language is required")
            .Length(1, 16)
            .WithMessage("Language must be between 1 and 16 characters");

        RuleFor(x => x.LanguageVersion)
            .NotEmpty()
            .WithMessage("Language version is required");

        RuleFor(x => x.SourceCode)
            .NotEmpty()
            .WithMessage("Source code is required")
            .Length(1, 10000)
            .WithMessage("Source code must be between 1 and 10000 characters")
            .Custom((code, context) =>
            {
                // Check for null bytes and other binary data
                if (code.Contains('\0'))
                {
                    context.AddFailure("Source code contains invalid binary data");
                }
            });
    }
}
