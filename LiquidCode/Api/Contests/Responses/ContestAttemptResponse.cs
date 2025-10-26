using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Ответ с информацией о попытке пользователя в контесте
/// </summary>
public record ContestAttemptResponse(
    int ContestId,
    int UserId,
    ContestScheduleType ScheduleType,
    DateTime StartedAt,
    DateTime ExpiresAt,
    int AttemptCount
);
