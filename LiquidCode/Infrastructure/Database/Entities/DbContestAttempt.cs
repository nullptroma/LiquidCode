using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Попытка участия пользователя в контесте
/// </summary>
[Index(nameof(ContestId), nameof(UserId), nameof(AttemptIndex), IsUnique = true)]
[Index(nameof(Status))]
[Index(nameof(StartedAt))]
[Index(nameof(ExpiresAt))]
public class DbContestAttempt : ITimestamped
{
    public int Id { get; set; }

    public int ContestId { get; set; }
    public DbContest Contest { get; set; } = null!;

    public int UserId { get; set; }
    public DbUser User { get; set; } = null!;

    public DbContestMembership Membership { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int AttemptIndex { get; set; }

    public ContestAttemptStatus Status { get; set; } = ContestAttemptStatus.Active;

    public ContestAttemptFinishReason? FinishedBy { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public decimal TotalScore { get; set; }
    public int SolvedCount { get; set; }

    public ICollection<DbContestAttemptMissionResult> MissionResults { get; init; } = new HashSet<DbContestAttemptMissionResult>();
    public ICollection<DbUserSubmission> Submissions { get; init; } = new HashSet<DbUserSubmission>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ContestAttemptStatus
{
    Active = 0,
    Completed = 1,
    Aborted = 2,
    Expired = 3
}

public enum ContestAttemptFinishReason
{
    Timer = 0,
    Manual = 1,
    Admin = 2,
    System = 3
}
