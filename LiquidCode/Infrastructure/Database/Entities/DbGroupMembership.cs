using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Пользователь, состоящий в группе
/// </summary>
[Index(nameof(Role))]
[PrimaryKey(nameof(GroupId), nameof(UserId))]
public class DbGroupMembership : ITimestamped
{
    public int GroupId { get; init; }
    public DbGroup Group { get; init; } = null!;
    
    public int UserId { get; init; }
    public DbUser User { get; init; } = null!;
    
    public GroupMembershipRole Role { get; set; } = GroupMembershipRole.Member;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Flags]
public enum GroupMembershipRole
{
    None = 0,
    Member = 1,
    Administrator = 2
}
