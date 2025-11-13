using FluentValidation;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Валидатор для запросов создания группы
/// </summary>
public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Group name is required")
            .Length(3, 128)
            .WithMessage("Group name must be between 3 and 128 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_.()]+$")
            .WithMessage("Group name contains invalid characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
