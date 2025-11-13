using FluentValidation;

namespace LiquidCode.Api.Groups.Requests;

public class CreateGroupFeedPostRequestValidator : AbstractValidator<CreateGroupFeedPostRequest>
{
    public CreateGroupFeedPostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content must not be empty");
    }
}
