using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Информация об участнике группы
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Username">Имя пользователя</param>
/// <param name="Role">Роль в группе</param>
/// <param name="JoinedAt">Дата и время присоединения к группе</param>
/// <param name="IsAutoJoined">Присоединен ли автоматически</param>
public record GroupMemberResponse(int UserId, string Username, GroupMembershipRole Role, DateTime JoinedAt, bool IsAutoJoined);
