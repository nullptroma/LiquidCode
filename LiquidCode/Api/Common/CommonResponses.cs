using System.ComponentModel.DataAnnotations;

namespace LiquidCode.Models.Dto;

/// <summary>
/// DTO for successful authentication response
/// </summary>
public record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn = 120);

/// <summary>
/// DTO for error response
/// </summary>
public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null,
    DateTime Timestamp = default)
{
    public ErrorResponse(int statusCode, string message) : this(statusCode, message, null, DateTime.UtcNow) { }
}

/// <summary>
/// DTO for paginated response
/// </summary>
public record PaginatedResponse<T>(
    IEnumerable<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);
