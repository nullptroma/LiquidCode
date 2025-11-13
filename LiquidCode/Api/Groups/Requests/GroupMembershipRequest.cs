using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на управление участником группы
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Role">Роль пользователя в группе</param>
public record GroupMembershipRequest(
    [Required] int UserId,
    GroupMembershipRole Role
);
