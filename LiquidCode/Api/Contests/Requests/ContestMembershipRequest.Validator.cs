using FluentValidation;
using LiquidCode.Api.Shared;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Валидатор для управления участниками контеста.
/// </summary>
public class ContestMembershipRequestValidator : AbstractValidator<ContestMembershipRequest>
{
    public ContestMembershipRequestValidator()
    {
        RuleFor(x => x.UserId)
            .OptionalPositiveId("User ID");

        RuleFor(x => x.Role)
            .OptionalEnumValue("Role");
    }
}
