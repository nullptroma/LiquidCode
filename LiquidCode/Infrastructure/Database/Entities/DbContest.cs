using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Контест, объединяющий миссии и статьи в рамках временного окна
/// </summary>
[Index(nameof(StartsAt))]
[Index(nameof(EndsAt))]
[Index(nameof(Visibility))]
[Index(nameof(IsDeleted))]
public class DbContest : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    [StringLength(ValidationLengths.Contest.NameMax, MinimumLength = ValidationLengths.Contest.NameMin)]
    public string Name { get; set; } = "";
    
    [StringLength(ValidationLengths.Contest.DescriptionMax)]
    public string? Description { get; set; }
    
    public ContestScheduleType ScheduleType { get; set; } = ContestScheduleType.FixedWindow;

    public ContestVisibility Visibility { get; set; } = ContestVisibility.Public;

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public int? AttemptDurationMinutes { get; set; }

    public int? MaxAttempts { get; set; } = 1;

    public bool AllowEarlyFinish { get; set; } = true;
    
    public int? GroupId { get; set; }
    public DbGroup? Group { get; set; }
    
    public ICollection<DbContestMission> Missions { get; init; } = new HashSet<DbContestMission>();
    public ICollection<DbContestArticle> Articles { get; init; } = new HashSet<DbContestArticle>();
    public ICollection<DbContestMembership> Memberships { get; init; } = new HashSet<DbContestMembership>();
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Тип временного окна контеста
/// </summary>
public enum ContestScheduleType
{
    AlwaysOpen = 0,
    FixedWindow = 1,
    RollingWindow = 2
}

public enum ContestVisibility
{
    Public = 0,
    GroupPrivate = 1
}
