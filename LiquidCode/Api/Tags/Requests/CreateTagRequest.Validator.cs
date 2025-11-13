using FluentValidation;
using LiquidCode.Api.Shared;

namespace LiquidCode.Api.Tags.Requests;

/// <summary>
/// Валидатор для создания тега.
/// </summary>
public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .ValidTagName();
    }
}
