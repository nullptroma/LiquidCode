namespace LiquidCode.Api.Profile.Responses;

/// <summary>
/// Ответ со списком статей пользователя
/// </summary>
/// <param name="Articles">Статьи c пагинацией</param>
public record ProfileArticlesResponse(ProfilePagedResponse<ProfileArticleResponse> Articles);

public record ProfileArticleResponse(
    int ArticleId,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt);
