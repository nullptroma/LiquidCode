using System;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Активный токен присоединения к группе
/// </summary>
public record GroupJoinLinkResponse(string Token, DateTime ExpiresAt);
