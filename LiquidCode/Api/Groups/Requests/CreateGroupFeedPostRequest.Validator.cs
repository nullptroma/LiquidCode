using FluentValidation;

namespace LiquidCode.Api.Groups.Requests;

public class CreateGroupFeedPostRequestValidator : AbstractValidator<CreateGroupFeedPostRequest>
{
    public CreateGroupFeedPostRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(256)
            .WithMessage("Post name must not exceed 256 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content must not be empty")
            .MaximumLength(50000)
            .WithMessage("Content must not exceed 50000 characters");
    }
}
