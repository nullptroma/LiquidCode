namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Interface for entities that support soft deletion
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been soft deleted
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was soft deleted (null if not deleted)
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
