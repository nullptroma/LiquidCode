using System;

namespace LiquidCode.Api.Contests.Responses;

/// <summary>
/// Информация о результате миссии в рамках попытки контеста
/// </summary>
/// <param name="MissionId">Идентификатор миссии</param>
/// <param name="MissionName">Название миссии</param>
/// <param name="SolvedAt">Дата и время успешного решения миссии</param>
/// <param name="IsSolved">Решена ли миссия успешно</param>
/// <param name="HighestScore">Наивысший балл среди всех попыток</param>
/// <param name="SubmissionCount">Количество отправленных решений в рамках этой попытки</param>
/// <param name="Penalty">Штрафное время (в минутах или попытках, в зависимости от правил контеста)</param>
/// <param name="FirstAcceptedAt">Дата и время первого успешно принятого решения</param>
/// <param name="LastSubmissionAt">Дата и время последней отправки решения</param>
/// <param name="BestSubmissionId">Идентификатор лучшей (самой высокобалльной) отправки</param>
public record ContestAttemptMissionResultResponse(
    int MissionId,
    string MissionName,
    DateTime? SolvedAt,
    bool IsSolved,
    decimal HighestScore,
    int SubmissionCount,
    double Penalty,
    DateTime? FirstAcceptedAt,
    DateTime? LastSubmissionAt,
    int? BestSubmissionId
);
