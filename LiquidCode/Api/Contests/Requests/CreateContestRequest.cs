using System.ComponentModel.DataAnnotations;
using LiquidCode.Infrastructure.Database.Entities;
using LiquidCode.Shared.Validation;

namespace LiquidCode.Api.Contests.Requests;

/// <summary>
/// Запрос на создание нового контеста
/// </summary>
/// <param name="Name">Название контеста</param>
/// <param name="Description">Описание контеста</param>
/// <param name="ScheduleType">Тип расписания контеста</param>
/// <param name="Visibility">Видимость контеста</param>
/// <param name="StartsAt">Дата и время начала контеста</param>
/// <param name="EndsAt">Дата и время окончания контеста</param>
/// <param name="AttemptDurationMinutes">Длительность одной попытки в минутах</param>
/// <param name="MaxAttempts">Максимальное количество попыток</param>
/// <param name="AllowEarlyFinish">Разрешить досрочное завершение попытки</param>
/// <param name="GroupId">Идентификатор группы, к которой привязан контест</param>
/// <param name="MissionIds">Идентификаторы миссий в контесте</param>
/// <param name="ArticleIds">Идентификаторы статей в контесте</param>
public record CreateContestRequest(
    [Required] [StringLength(ValidationLengths.Contest.NameMax, MinimumLength = ValidationLengths.Contest.NameMin)] string Name,
    [StringLength(ValidationLengths.Contest.DescriptionMax)] string? Description,
    ContestScheduleType ScheduleType,
    ContestVisibility Visibility,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int AttemptDurationMinutes,
    int MaxAttempts,
    bool AllowEarlyFinish,
    int? GroupId,
    IEnumerable<int>? MissionIds,
    IEnumerable<int>? ArticleIds
);
