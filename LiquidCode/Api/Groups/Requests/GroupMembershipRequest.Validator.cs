using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Валидатор запросов изменения роли участника группы.
/// </summary>
public class GroupMembershipRequestValidator : AbstractValidator<GroupMembershipRequest>
{
    public GroupMembershipRequestValidator()
    {
        RuleFor(x => x.UserId)
            .PositiveId("User ID");

        RuleFor(x => x.Role)
            .ValidEnumValue("Role");
    }
}
