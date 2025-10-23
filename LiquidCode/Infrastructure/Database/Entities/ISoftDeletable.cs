namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Интерфейс для сущностей, которые поддерживают мягкое удаление
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Указывает, была ли сущность мягко удалена
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// Временная метка, когда сущность была мягко удалена (null, если не удалена)
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
