using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Описание участника или организатора контеста
/// </summary>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="Username">Имя пользователя</param>
/// <param name="Role">Роль в контесте</param>
public record ContestMemberResponse(int UserId, string Username, ContestMembershipRole Role);
