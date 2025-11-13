using FluentValidation;

namespace LiquidCode.Api.Groups.Requests;

public class UpdateGroupFeedPostRequestValidator : AbstractValidator<UpdateGroupFeedPostRequest>
{
    public UpdateGroupFeedPostRequestValidator()
    {
        RuleFor(x => x.Content)
            .Must(content => content == null || !string.IsNullOrWhiteSpace(content))
            .WithMessage("Content must not be empty when provided");
    }
}
