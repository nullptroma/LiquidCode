using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Приглашение пользователя в группу
/// </summary>
[Index(nameof(GroupId), nameof(InviteeId), nameof(Status))]
[Index(nameof(Token), IsUnique = true)]
[Index(nameof(ExpiresAt))]
public class DbGroupInvitation : ITimestamped
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public DbGroup Group { get; set; } = null!;

    public int InviterId { get; set; }
    public DbUser Inviter { get; set; } = null!;

    public int InviteeId { get; set; }
    public DbUser Invitee { get; set; } = null!;

    [StringLength(128)]
    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    public GroupInvitationStatus Status { get; set; } = GroupInvitationStatus.Pending;

    public GroupInvitationDeliveryChannel DeliveryChannel { get; set; } = GroupInvitationDeliveryChannel.Username;

    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum GroupInvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Expired = 3,
    Revoked = 4
}

public enum GroupInvitationDeliveryChannel
{
    Username = 0,
    Email = 1,
    DirectLink = 2
}
