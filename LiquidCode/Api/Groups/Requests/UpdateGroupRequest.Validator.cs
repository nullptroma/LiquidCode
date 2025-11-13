using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Валидатор для обновления группы.
/// </summary>
public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .OptionalTextWithinRange("Group name", ValidationLengths.Group.Name);

        RuleFor(x => x.Description)
            .OptionalTextWhenProvided("Description", ValidationLengths.Group.DescriptionMax);

        RuleFor(x => x)
            .Must(HasAnyField)
            .WithMessage("At least one field must be provided");
    }

    private static bool HasAnyField(UpdateGroupRequest request)
    {
        return request.Name != null || request.Description != null;
    }
}
