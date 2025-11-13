using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на управление участниками контеста
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Role">Роль пользователя в контесте</param>
public record ContestMembershipRequest(
    int? UserId,
    ContestMembershipRole? Role
);
