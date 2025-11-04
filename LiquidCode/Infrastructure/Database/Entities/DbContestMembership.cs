using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Пользователь контеста с ролью участника или организатора
/// </summary>
[Index(nameof(Role))]
[PrimaryKey(nameof(ContestId), nameof(UserId))]
public class DbContestMembership : ITimestamped
{
    public int ContestId { get; init; }
    public DbContest Contest { get; init; } = null!;
    
    public int UserId { get; init; }
    public DbUser User { get; init; } = null!;
    
    public ContestMembershipRole Role { get; set; } = ContestMembershipRole.Participant;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsAutoJoined { get; set; }

    public int? InvitationId { get; set; }
    public DbGroupInvitation? Invitation { get; set; }

    public int? ActiveAttemptId { get; set; }
    public DbContestAttempt? ActiveAttempt { get; set; }

    public DateTime? LastAttemptStartedAt { get; set; }

    public ICollection<DbContestAttempt> Attempts { get; init; } = new HashSet<DbContestAttempt>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Flags]
public enum ContestMembershipRole
{
    None = 0,
    Participant = 1,
    Organizer = 2
}
