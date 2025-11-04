using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Результат решения конкретной задачи в рамках попытки контеста
/// </summary>
[PrimaryKey(nameof(ContestAttemptId), nameof(MissionId))]
[Index(nameof(MissionId))]
public class DbContestAttemptMissionResult : ITimestamped
{
    public int ContestAttemptId { get; set; }
    public DbContestAttempt ContestAttempt { get; set; } = null!;

    public int MissionId { get; set; }
    public DbMission Mission { get; set; } = null!;

    public DateTime? SolvedAt { get; set; }

    public int SubmissionCount { get; set; }

    public decimal HighestScore { get; set; }

    public double Penalty { get; set; }

    public DateTime? LastSubmissionAt { get; set; }

    public DateTime? FirstAcceptedAt { get; set; }

    public int? BestSubmissionId { get; set; }
    public DbUserSubmission? BestSubmission { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
