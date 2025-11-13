using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Валидатор для запросов создания группы
/// </summary>
public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredText("Group name", ValidationLengths.Group.Name)
            .Matches(@"^[a-zA-Z0-9\s\-_.()]+$")
            .WithMessage("Group name contains invalid characters");

        RuleFor(x => x.Description)
            .OptionalTextWhenProvided("Description", ValidationLengths.Group.DescriptionMax);
    }
}
