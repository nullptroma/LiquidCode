using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на управление участниками контеста
/// </summary>
public record ContestMembershipRequest(
    int? UserId,
    ContestMembershipRole? Role
);
