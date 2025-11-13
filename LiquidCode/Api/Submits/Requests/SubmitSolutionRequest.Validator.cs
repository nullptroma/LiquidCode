using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Submits.Requests;

/// <summary>
/// Валидатор для запросов отправки решений
/// </summary>
public class SubmitSolutionRequestValidator : AbstractValidator<SubmitSolutionRequest>
{
    public SubmitSolutionRequestValidator()
    {
        RuleFor(x => x.MissionId)
            .PositiveId("Mission ID");

        RuleFor(x => x.ContestId)
            .OptionalPositiveId("Contest ID");

        RuleFor(x => x.Language)
            .RequiredText("Programming language", ValidationLengths.Solution.Language);

        RuleFor(x => x.LanguageVersion)
            .RequiredText("Language version", ValidationLengths.Solution.LanguageVersion);

        RuleFor(x => x.SourceCode)
            .RequiredText("Source code", ValidationLengths.Solution.SourceCode)
            .Custom((code, context) =>
            {
                // Проверить на нулевые байты и другие двоичные данные
                if (code.Contains('\0'))
                {
                    context.AddFailure("Source code contains invalid binary data");
                }
            });
    }
}
