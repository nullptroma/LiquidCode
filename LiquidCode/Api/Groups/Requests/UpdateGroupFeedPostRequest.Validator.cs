using FluentValidation;

namespace LiquidCode.Api.Groups.Requests;

public class UpdateGroupFeedPostRequestValidator : AbstractValidator<UpdateGroupFeedPostRequest>
{
    public UpdateGroupFeedPostRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(256)
            .WithMessage("Post name must not exceed 256 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Content)
            .Must(content => content == null || !string.IsNullOrWhiteSpace(content))
            .WithMessage("Content must not be empty when provided")
            .MaximumLength(50000)
            .WithMessage("Content must not exceed 50000 characters")
            .When(x => x.Content != null);
    }
}
