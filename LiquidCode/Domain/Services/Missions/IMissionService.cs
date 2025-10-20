using LiquidCode.Api.Missions.Requests;
using LiquidCode.Api.Missions.Responses;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Domain.Services.Missions;

/// <summary>
/// Service interface for mission-related operations
/// </summary>
public interface IMissionService
{
    /// <summary>
    /// Uploads a new mission from a ZIP file
    /// </summary>
    /// <param name="form">Upload form with mission file and metadata</param>
    /// <param name="userId">ID of the user uploading the mission</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created mission model or null if upload failed</returns>
    Task<MissionResponse?> UploadMissionAsync(UploadMissionRequest form, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a public download link for a mission
    /// </summary>
    /// <param name="missionId">Mission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download URL or null if mission not found</returns>
    Task<string?> GetMissionDownloadLinkAsync(int missionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mission text data in a specific language
    /// </summary>
    /// <param name="missionId">Mission ID</param>
    /// <param name="language">Language code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mission text data as JSON string or null if not found</returns>
    Task<string?> GetMissionTextAsync(int missionId, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of missions
    /// </summary>
    /// <param name="pageSize">Number of missions per page</param>
    /// <param name="pageNumber">Zero-based page number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mission list with pagination info or null if invalid parameters</returns>
    Task<MissionsPageResponse?> GetMissionsListAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
}
