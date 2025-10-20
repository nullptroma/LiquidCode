using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for mission-related database operations
/// </summary>
public interface IMissionRepository : IRepository<DbMission>
{
    /// <summary>
    /// Gets missions with pagination
    /// </summary>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="pageNumber">Zero-based page number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (missions, hasNextPage)</returns>
    Task<(IEnumerable<DbMission> Missions, bool HasNextPage)> GetMissionsPageAsync(
        int pageSize, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets missions by author
    /// </summary>
    Task<IEnumerable<DbMission>> GetMissionsByAuthorAsync(int authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mission text data in a specific language
    /// </summary>
    Task<DbMissionPublicTextData?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available languages for a mission
    /// </summary>
    Task<IEnumerable<string>> GetMissionLanguagesAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds mission text data
    /// </summary>
    Task AddMissionTextAsync(DbMissionPublicTextData textData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple mission text data entries
    /// </summary>
    Task AddMissionTextsAsync(IEnumerable<DbMissionPublicTextData> textData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts total missions
    /// </summary>
    Task<int> CountMissionsAsync(CancellationToken cancellationToken = default);
}
