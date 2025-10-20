namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Interface for entities that track creation and update timestamps
/// </summary>
public interface ITimestamped
{
    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
