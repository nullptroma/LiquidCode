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
    string Content,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static ArticleResponse FromEntity(DbArticle entity)
    {
        var authorId = entity.Author?.Id ?? entity.AuthorId;
        var tags = entity.ArticleTags
            .Where(at => !string.IsNullOrWhiteSpace(at.Tag?.Name))
            .Select(at => at.Tag!.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        return new ArticleResponse(
            entity.Id,
            authorId,
            entity.Name,
            entity.Content,
            tags,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
