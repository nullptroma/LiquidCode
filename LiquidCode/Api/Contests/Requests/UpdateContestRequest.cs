using System.Collections.Generic;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на обновление контеста
/// </summary>
/// <param name="Name">Новое название контеста</param>
/// <param name="Description">Новое описание контеста</param>
/// <param name="ScheduleType">Новый тип расписания контеста</param>
/// <param name="Visibility">Новая видимость контеста</param>
/// <param name="StartsAt">Новая дата и время начала контеста</param>
/// <param name="EndsAt">Новая дата и время окончания контеста</param>
/// <param name="AttemptDurationMinutes">Новая длительность одной попытки в минутах</param>
/// <param name="MaxAttempts">Новое максимальное количество попыток</param>
/// <param name="AllowEarlyFinish">Разрешить досрочное завершение попытки</param>
/// <param name="GroupId">Новый идентификатор группы</param>
/// <param name="MissionIds">Новые идентификаторы миссий</param>
/// <param name="ArticleIds">Новые идентификаторы статей</param>
public record UpdateContestRequest(
    string? Name,
    string? Description,
    ContestScheduleType? ScheduleType,
    ContestVisibility? Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? AttemptDurationMinutes,
    int? MaxAttempts,
    bool? AllowEarlyFinish,
    int? GroupId,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds
);
