using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Groups.Requests;

public class UpdateGroupFeedPostRequestValidator : AbstractValidator<UpdateGroupFeedPostRequest>
{
    public UpdateGroupFeedPostRequestValidator()
    {
        RuleFor(x => x.Name)
            .OptionalTextWhenProvided("Post name", ValidationLengths.Group.FeedPostNameMax);

        RuleFor(x => x.Content)
            .OptionalNonEmptyText("Content", ValidationLengths.Group.FeedPostContentMax);
    }
}
