using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiquidCode.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Тег для статей и миссий
/// </summary>
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(IsDeleted))]
public class DbTag : ISoftDeletable, ITimestamped
{
    public int Id { get; set; }
    
    [StringLength(ValidationLengths.Tag.NameMax, MinimumLength = ValidationLengths.Tag.NameMin)]
    public string Name { get; set; } = "";
    
    public ICollection<DbMissionTag> MissionTags { get; init; } = new HashSet<DbMissionTag>();
    public ICollection<DbArticleTag> ArticleTags { get; init; } = new HashSet<DbArticleTag>();
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
