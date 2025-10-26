using Microsoft.AspNetCore.Http;

namespace LiquidCode.Api.Articles.Requests;

/// <summary>
/// Запрос на создание новой статьи
/// </summary>
public record CreateArticleRequest(
    IFormFile ContentArchive,
    string Name,
    IEnumerable<string>? Tags
);
