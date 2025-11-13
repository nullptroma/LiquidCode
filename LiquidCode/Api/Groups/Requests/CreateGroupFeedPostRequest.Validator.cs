using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Groups.Requests;

public class CreateGroupFeedPostRequestValidator : AbstractValidator<CreateGroupFeedPostRequest>
{
    public CreateGroupFeedPostRequestValidator()
    {
        RuleFor(x => x.Name)
            .OptionalText("Post name", ValidationLengths.Group.FeedPostNameMax);

        RuleFor(x => x.Content)
            .RequiredText("Content", ValidationLengths.Group.FeedPostContent);
    }
}
