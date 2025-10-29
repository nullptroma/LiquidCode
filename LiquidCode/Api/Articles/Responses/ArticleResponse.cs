using System.Collections.Generic;
using System.Linq;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Articles.Responses;

/// <summary>
/// Ответ API для статьи
/// </summary>
public record ArticleResponse(
    int Id,
    int AuthorId,
    string Name,
    string S3ContentKey,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static ArticleResponse FromEntity(DbArticle entity) => new(
        entity.Id,
        entity.Author.Id,
        entity.Name,
        entity.S3Key,
        entity.ArticleTags.Select(at => at.Tag.Name).Distinct().OrderBy(name => name).ToList(),
        entity.CreatedAt,
        entity.UpdatedAt
    );
}
