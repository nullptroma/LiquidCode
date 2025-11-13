using System;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Groups.Responses;

/// <summary>
/// Краткое описание контеста, созданного в группе
/// </summary>
/// <param name="ContestId">Идентификатор контеста</param>
/// <param name="Name">Название контеста</param>
/// <param name="ScheduleType">Тип расписания контеста</param>
/// <param name="Visibility">Видимость контеста</param>
/// <param name="StartsAt">Дата и время начала контеста</param>
/// <param name="EndsAt">Дата и время окончания контеста</param>
/// <param name="AttemptDurationMinutes">Длительность одной попытки в минутах</param>
/// <param name="MaxAttempts">Максимальное количество попыток</param>
/// <param name="AllowEarlyFinish">Разрешено ли досрочное завершение попытки</param>
public record GroupContestSummary(
    int ContestId,
    string Name,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool AllowEarlyFinish
);
