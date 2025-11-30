using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Валидатор для запросов создания контеста
/// </summary>
public class CreateContestRequestValidator : AbstractValidator<CreateContestRequest>
{
    public CreateContestRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredText("Contest name", ValidationLengths.Contest.Name);

        RuleFor(x => x.Description)
            .OptionalTextWhenProvided("Description", ValidationLengths.Contest.DescriptionMax);

        RuleFor(x => x.AttemptDurationMinutes)
            .GreaterThan(0)
            .WithMessage("Attempt duration must be positive")
            .LessThanOrEqualTo(43200) // 30 дней в минутах
            .WithMessage("Attempt duration must not exceed 30 days");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0)
            .WithMessage("Max attempts must be positive")
            .LessThanOrEqualTo(1000)
            .WithMessage("Max attempts must not exceed 1000");

        RuleFor(x => x.StartsAt)
            .LessThan(x => x.EndsAt)
            .WithMessage("Contest start time must be before end time")
            .When(x => x.StartsAt.HasValue && x.EndsAt.HasValue);

        RuleFor(x => x.MissionIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("All mission IDs must be positive")
            .When(x => x.MissionIds != null);

        RuleFor(x => x.ArticleIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("All article IDs must be positive")
            .When(x => x.ArticleIds != null);

        RuleFor(x => x.GroupId)
            .GreaterThan(0)
            .WithMessage("Group ID must be positive")
            .When(x => x.GroupId.HasValue);
    }
}
