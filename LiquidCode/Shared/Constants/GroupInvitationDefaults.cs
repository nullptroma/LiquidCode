using System;

namespace LiquidCode.Shared.Constants;

public static class GroupInvitationDefaults
{
    public static readonly TimeSpan InvitationTtl = TimeSpan.FromDays(7);
    public static readonly TimeSpan JoinTokenTtl = TimeSpan.FromDays(7);
}
