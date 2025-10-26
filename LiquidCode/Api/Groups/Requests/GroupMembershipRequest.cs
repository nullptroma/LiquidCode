using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Requests;

/// <summary>
/// Запрос на управление участником группы
/// </summary>
public record GroupMembershipRequest(
    [Required] int UserId,
    GroupMembershipRole Role
);
