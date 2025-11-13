using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Сущность пользователя с улучшенной индексацией и полями аудита
/// </summary>
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email))]
[Index(nameof(IsDeleted))]
public class DbUser : ISoftDeletable, ITimestamped
{
    public int Id { get; init; }
    
    [StringLength(ValidationLengths.User.UsernameMax, MinimumLength = ValidationLengths.User.UsernameMin)] 
    public string Username { get; init; } = "";
    
    [StringLength(ValidationLengths.User.EmailMax, MinimumLength = ValidationLengths.User.EmailMin)] 
    public string Email { get; init; } = "";
    
    [StringLength(ValidationLengths.User.PasswordHashMax)] 
    public string PassHash { get; init; } = "";
    
    // Поддержка мягкого удаления
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Временные метки
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}