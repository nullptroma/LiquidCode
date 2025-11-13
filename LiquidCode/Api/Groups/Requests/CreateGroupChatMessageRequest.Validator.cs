using FluentValidation;
using LiquidCode.Shared.Constants;

namespace LiquidCode.Api.Groups.Requests;

public class CreateGroupChatMessageRequestValidator : AbstractValidator<CreateGroupChatMessageRequest>
{
    public CreateGroupChatMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content must not be empty")
            .MaximumLength(GroupCommunicationDefaults.ChatMessageMaxLength);
    }
}
