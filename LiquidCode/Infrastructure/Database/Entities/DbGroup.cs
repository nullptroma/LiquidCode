using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Группа пользователей для организации учебного процесса
/// </summary>
[Index(nameof(IsDeleted))]
public class DbGroup : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    [StringLength(ValidationLengths.Group.NameMax, MinimumLength = ValidationLengths.Group.NameMin)]
    public string Name { get; set; } = "";
    
    [StringLength(ValidationLengths.Group.DescriptionMax)]
    public string? Description { get; set; }
    
    public ICollection<DbGroupMembership> Memberships { get; init; } = new HashSet<DbGroupMembership>();
    public ICollection<DbContest> Contests { get; init; } = new HashSet<DbContest>();
    public ICollection<DbGroupInvitation> Invitations { get; init; } = new HashSet<DbGroupInvitation>();
    public ICollection<DbGroupJoinToken> JoinTokens { get; init; } = new HashSet<DbGroupJoinToken>();
    public ICollection<DbGroupFeedPost> FeedPosts { get; init; } = new HashSet<DbGroupFeedPost>();
    public ICollection<DbGroupChatMessage> ChatMessages { get; init; } = new HashSet<DbGroupChatMessage>();
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
