using FluentValidation;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Валидатор для запросов обновления контеста
/// </summary>
public class UpdateContestRequestValidator : AbstractValidator<UpdateContestRequest>
{
    public UpdateContestRequestValidator()
    {
        RuleFor(x => x.Name)
            .Length(3, 128)
            .WithMessage("Contest name must be between 3 and 128 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .WithMessage("Description must not exceed 5000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.AttemptDurationMinutes)
            .GreaterThan(0)
            .WithMessage("Attempt duration must be positive")
            .LessThanOrEqualTo(43200) // 30 дней в минутах
            .WithMessage("Attempt duration must not exceed 30 days")
            .When(x => x.AttemptDurationMinutes.HasValue);

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0)
            .WithMessage("Max attempts must be positive")
            .LessThanOrEqualTo(1000)
            .WithMessage("Max attempts must not exceed 1000")
            .When(x => x.MaxAttempts.HasValue);

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
