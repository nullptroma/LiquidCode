using System.Collections.Generic;
using System.Linq;
using LiquidCode.Infrastructure.Database.Entities;

namespace LiquidCode.Api.Articles.Responses;

/// <summary>
/// Ответ API для статьи
/// </summary>
/// <param name="Id">Идентификатор статьи</param>
/// <param name="AuthorId">Идентификатор автора</param>
/// <param name="Name">Название статьи</param>
/// <param name="Content">Содержимое статьи (может содержать разметку или HTML)</param>
/// <param name="Tags">Список тегов статьи, отсортированный по алфавиту</param>
/// <param name="CreatedAt">Дата и время создания</param>
/// <param name="UpdatedAt">Дата и время последнего обновления</param>
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
