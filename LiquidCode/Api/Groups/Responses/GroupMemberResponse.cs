using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Информация об участнике группы
/// </summary>
public record GroupMemberResponse(int UserId, string Username, GroupMembershipRole Role, DateTime JoinedAt, bool IsAutoJoined);
