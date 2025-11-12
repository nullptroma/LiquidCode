using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Описание участника или организатора контеста
/// </summary>
public record ContestMemberResponse(int UserId, string Username, ContestMembershipRole Role);
