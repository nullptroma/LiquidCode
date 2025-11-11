using System;

namespace LiquidCode.Shared.Constants;

public static class GroupDefaults
{
    /// <summary>
    /// Срок действия ссылки-присоединения. По истечении срока выдаётся новый токен.
    /// </summary>
    public static readonly TimeSpan JoinLinkLifetime = TimeSpan.FromDays(1);
}
