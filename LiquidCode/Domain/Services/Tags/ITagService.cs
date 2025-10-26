using LiquidCode.Api.Tags.Responses;

namespace LiquidCode.Domain.Services.Tags;

/// <summary>
/// Сервис управления тегами
/// </summary>
public interface ITagService
{
    Task<TagResponse?> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TagResponse>> SearchAsync(string? query, int limit, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int tagId, CancellationToken cancellationToken = default);
}
