using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Токен для присоединения к группе по ссылке
/// </summary>
[Index(nameof(GroupId))]
[Index(nameof(Token), IsUnique = true)]
[Index(nameof(ExpiresAt))]
public class DbGroupJoinToken : ITimestamped
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public DbGroup Group { get; set; } = null!;

    public int CreatedById { get; set; }
    public DbUser CreatedBy { get; set; } = null!;

    [StringLength(128)]
    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public DateTime? LastRefreshedAt { get; set; }

    public int UsageCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
