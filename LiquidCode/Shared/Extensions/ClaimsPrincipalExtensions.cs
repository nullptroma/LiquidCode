using System.Security.Claims;

namespace LiquidCode.Shared.Extensions;

/// <summary>
/// Extension methods for working with ClaimsPrincipal (User claims)
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Attempts to extract the user ID from claims
    /// </summary>
    /// <param name="user">The claims principal to extract from</param>
    /// <param name="userId">Output parameter for the extracted user ID</param>
    /// <returns>True if user ID was found and parsed successfully, false otherwise</returns>
    public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
    {
        userId = 0;
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out userId);
    }

    /// <summary>
    /// Gets the user ID from claims, or returns null if not found
    /// </summary>
    public static int? GetUserIdOrNull(this ClaimsPrincipal user)
    {
        return user.TryGetUserId(out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the username from claims
    /// </summary>
    public static string? GetUsername(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Gets the email from claims
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Email)?.Value;
}
