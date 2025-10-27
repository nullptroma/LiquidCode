using LiquidCode.Api.Articles.Requests;
using LiquidCode.Api.Articles.Responses;

namespace LiquidCode.Domain.Interfaces.Services;

/// <summary>
/// Сервис управления статьями
/// </summary>
public interface IArticleService
{
    Task<ArticleResponse?> CreateAsync(CreateArticleRequest request, int authorId, CancellationToken cancellationToken = default);
    Task<ArticleResponse?> UpdateAsync(int articleId, UpdateArticleRequest request, int userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int articleId, int userId, CancellationToken cancellationToken = default);
    Task<ArticleResponse?> GetAsync(int articleId, CancellationToken cancellationToken = default);
    Task<ArticlesPageResponse?> GetPageAsync(int pageSize, int pageNumber, IEnumerable<string>? tags, CancellationToken cancellationToken = default);
}
