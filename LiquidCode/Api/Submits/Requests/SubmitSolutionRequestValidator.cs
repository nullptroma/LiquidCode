using FluentValidation;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Validator for solution submission requests
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
            .WithMessage("Language must be between 1 and 16 characters")
            .Must(lang => AppConstants.SupportedLanguages.Contains(lang.ToLowerInvariant()))
            .WithMessage($"Supported languages are: {string.Join(", ", AppConstants.SupportedLanguages)}");

        RuleFor(x => x.LanguageVersion)
            .NotEmpty()
            .WithMessage("Language version is required")
            .Length(1, 16)
            .WithMessage("Language version must be between 1 and 16 characters")
            .Matches(@"^\d+(\.\d+)*$|^latest$|^default$")
            .WithMessage("Language version must be in format like '1.0', '2.3.1' or 'latest'");

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
