namespace LiquidCode.Infrastructure.Database.Entities;

/// <summary>
/// Интерфейс для сущностей, которые отслеживают временные метки создания и обновления
/// </summary>
public interface ITimestamped
{
    /// <summary>
    /// Временная метка, когда сущность была создана
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Временная метка, когда сущность была последний раз обновлена
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
